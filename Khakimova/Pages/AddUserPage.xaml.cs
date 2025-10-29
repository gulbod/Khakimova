using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using Microsoft.Win32;

namespace Khakimova.Pages
{
    public partial class AddUserPage : Page
    {
        private readonly User _currentUser;
        private string _photoFileName;
        private byte[] _imageData;
        private readonly string _photosDirectory;

        public AddUserPage(User selectedUser = null)
        {
            InitializeComponent();
            _currentUser = selectedUser;
            _photosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UserPhotos");

            if (!Directory.Exists(_photosDirectory))
            {
                Directory.CreateDirectory(_photosDirectory);
            }

            if (_currentUser != null)
            {
                LoadUserData();
            }
            else
            {
                ClearForm();
            }
        }

        private void LoadUserData()
        {
            try
            {
                LoginTextBox.Text = _currentUser.Login;
                FioTextBox.Text = _currentUser.FIO;

                // Установка роли
                foreach (ComboBoxItem item in RoleComboBox.Items)
                {
                    if (item.Content?.ToString() == _currentUser.Role)
                    {
                        RoleComboBox.SelectedItem = item;
                        break;
                    }
                }

                // Загрузка фото
                if (!string.IsNullOrEmpty(_currentUser.Photo))
                {
                    _photoFileName = _currentUser.Photo;
                    LoadImageFromFile(_photoFileName);
                    PhotoNameTextBlock.Text = $"Фото: {Path.GetFileName(_photoFileName)}";
                    NoImagePreviewText.Visibility = Visibility.Collapsed;
                }
                else
                {
                    ClearImage();
                }

                Title = "Редактирование пользователя";
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных пользователя: {ex.Message}");
            }
        }

        private void ClearForm()
        {
            LoginTextBox.Clear();
            FioTextBox.Clear();
            RoleComboBox.SelectedIndex = -1;
            ClearImage();
            PhotoNameTextBlock.Text = "Фото не выбрано";
            Title = "Добавление пользователя";
        }

        private void ClearImage()
        {
            UserPhotoImage.Source = null;
            _photoFileName = null;
            _imageData = null;
            NoImagePreviewText.Visibility = Visibility.Visible;
        }

        private void SelectPhotoButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                var openFileDialog = new OpenFileDialog
                {
                    Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp|All files (*.*)|*.*",
                    FilterIndex = 1
                };

                if (openFileDialog.ShowDialog() == true)
                {
                    string filePath = openFileDialog.FileName;
                    _photoFileName = filePath;

                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.EndInit();

                    UserPhotoImage.Source = bitmap;
                    NoImagePreviewText.Visibility = Visibility.Collapsed;

                    _imageData = File.ReadAllBytes(filePath);
                    PhotoNameTextBlock.Text = $"Фото: {Path.GetFileName(filePath)}";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при выборе фото: {ex.Message}");
            }
        }

        private void LoadImageFromFile(string fileName)
        {
            try
            {
                string filePath = Path.Combine(_photosDirectory, fileName);

                if (File.Exists(filePath))
                {
                    var bitmap = new BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(filePath);
                    bitmap.EndInit();
                    UserPhotoImage.Source = bitmap;
                    NoImagePreviewText.Visibility = Visibility.Collapsed;
                    _photoFileName = fileName;
                }
                else
                {
                    ClearImage();
                    PhotoNameTextBlock.Text = "Фото не найдено";
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
                ClearImage();
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                // Валидация данных
                if (string.IsNullOrWhiteSpace(LoginTextBox.Text) ||
                    string.IsNullOrWhiteSpace(FioTextBox.Text) ||
                    RoleComboBox.SelectedItem == null)
                {
                    MessageBox.Show("Заполните все обязательные поля!", "Ошибка",
                                  MessageBoxButton.OK, MessageBoxImage.Warning);
                    return;
                }

                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    User userToSave;

                    if (_currentUser != null)
                    {
                        userToSave = db.User.FirstOrDefault(u => u.ID == _currentUser.ID);
                        if (userToSave == null)
                        {
                            MessageBox.Show("Пользователь не найден в базе данных!", "Ошибка");
                            return;
                        }
                    }
                    else
                    {
                        userToSave = new User();
                        db.User.Add(userToSave);
                    }

                    userToSave.Login = LoginTextBox.Text.Trim();
                    userToSave.FIO = FioTextBox.Text.Trim();
                    userToSave.Role = (RoleComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString();

                    if (!string.IsNullOrEmpty(_photoFileName) && _imageData != null)
                    {
                        string fileExtension = Path.GetExtension(_photoFileName);
                        string uniqueFileName = $"{Guid.NewGuid()}{fileExtension}";
                        string destinationPath = Path.Combine(_photosDirectory, uniqueFileName);

                        File.WriteAllBytes(destinationPath, _imageData);
                        userToSave.Photo = uniqueFileName;
                    }
                    else if (!string.IsNullOrEmpty(_photoFileName))
                    {
                        userToSave.Photo = Path.GetFileName(_photoFileName);
                    }
                    else
                    {
                        userToSave.Photo = null;
                    }

                    db.SaveChanges();

                    string message = _currentUser != null ?
                        "Данные пользователя успешно обновлены!" :
                        "Пользователь успешно добавлен!";

                    MessageBox.Show(message, "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);

                    if (NavigationService?.CanGoBack == true)
                    {
                        NavigationService.GoBack();
                    }
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения данных: {ex.Message}", "Ошибка");
            }
        }

        private void ClearButton_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите очистить все поля?", "Подтверждение",
                              MessageBoxButton.YesNo, MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                ClearForm();
            }
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService?.CanGoBack == true)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет предыдущей страницы для возврата.", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}