using System;
using System.Linq;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using Microsoft.Win32;
using System.IO;

namespace Khakimova.Pages
{
    public partial class AddUserPage : Page
    {
        private User _currentUser = new User();
        private string _selectedPhotoPath = "";

        public AddUserPage(User selectedUser)
        {
            InitializeComponent();

            if (selectedUser != null)
            {
                _currentUser = selectedUser;

                // Установка выбранной роли в ComboBox
                foreach (ComboBoxItem item in cmbRole.Items)
                {
                    if (item.Content.ToString() == _currentUser.Role)
                    {
                        cmbRole.SelectedItem = item;
                        break;
                    }
                }

                // Загрузка существующего фото
                if (!string.IsNullOrEmpty(_currentUser.Photo))
                {
                    LoadUserImage(_currentUser.Photo);
                }
            }

            DataContext = _currentUser;

            // Если роль не установлена, выбираем первую по умолчанию
            if (cmbRole.SelectedItem == null && cmbRole.Items.Count > 0)
            {
                cmbRole.SelectedIndex = 0;
            }
        }

        private void ButtonSelectPhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog();
            openFileDialog.Filter = "Image files (*.jpg; *.jpeg; *.png; *.bmp)|*.jpg; *.jpeg; *.png; *.bmp|All files (*.*)|*.*";
            openFileDialog.FilterIndex = 1;

            if (openFileDialog.ShowDialog() == true)
            {
                _selectedPhotoPath = openFileDialog.FileName;
                TBPhotoName.Text = Path.GetFileName(_selectedPhotoPath);
                LoadUserImage(_selectedPhotoPath);
            }
        }

        private void LoadUserImage(string imagePath)
        {
            try
            {
                if (File.Exists(imagePath))
                {
                    var bitmap = new System.Windows.Media.Imaging.BitmapImage();
                    bitmap.BeginInit();
                    bitmap.UriSource = new Uri(imagePath);
                    bitmap.CacheOption = System.Windows.Media.Imaging.BitmapCacheOption.OnLoad;
                    bitmap.EndInit();
                    UserImage.Source = bitmap;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки изображения: {ex.Message}");
            }
        }

        private string SaveUserPhoto()
        {
            if (string.IsNullOrEmpty(_selectedPhotoPath) || !File.Exists(_selectedPhotoPath))
                return _currentUser.Photo;

            try
            {
                // Создаем папку для фотографий пользователей, если её нет
                string photosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UserPhotos");
                if (!Directory.Exists(photosDirectory))
                {
                    Directory.CreateDirectory(photosDirectory);
                }

                // Генерируем уникальное имя файла
                string fileExtension = Path.GetExtension(_selectedPhotoPath);
                string fileName = $"user_{_currentUser.ID}_{DateTime.Now:yyyyMMddHHmmss}{fileExtension}";
                string destinationPath = Path.Combine(photosDirectory, fileName);

                // Копируем файл
                File.Copy(_selectedPhotoPath, destinationPath, true);

                return destinationPath;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения фотографии: {ex.Message}");
                return _currentUser.Photo;
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(_currentUser.Login))
                errors.AppendLine("Укажите логин!");
            if (string.IsNullOrWhiteSpace(_currentUser.Password))
                errors.AppendLine("Укажите пароль!");
            if (cmbRole.SelectedItem == null)
                errors.AppendLine("Выберите роль!");
            else
                _currentUser.Role = (cmbRole.SelectedItem as ComboBoxItem)?.Content.ToString();
            if (string.IsNullOrWhiteSpace(_currentUser.FIO))
                errors.AppendLine("Укажите ФИО!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    // Сохраняем фото перед сохранением пользователя
                    if (!string.IsNullOrEmpty(_selectedPhotoPath))
                    {
                        _currentUser.Photo = SaveUserPhoto();
                    }

                    if (_currentUser.ID == 0)
                    {
                        db.User.Add(_currentUser);
                    }
                    else
                    {
                        var existingUser = db.User.FirstOrDefault(u => u.ID == _currentUser.ID);
                        if (existingUser != null)
                        {
                            existingUser.Login = _currentUser.Login;
                            existingUser.Password = _currentUser.Password;
                            existingUser.Role = _currentUser.Role;
                            existingUser.FIO = _currentUser.FIO;
                            existingUser.Photo = _currentUser.Photo;
                        }
                    }

                    db.SaveChanges();
                    MessageBox.Show("Данные успешно сохранены!");
                    NavigationService?.GoBack();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TBLogin.Text = "";
            TBPass.Text = "";
            cmbRole.SelectedIndex = -1;
            TBFio.Text = "";
            TBPhotoName.Text = "";
            UserImage.Source = null;
            _selectedPhotoPath = "";
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}