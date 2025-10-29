using System;
using System.IO;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;

namespace Khakimova.Pages
{
    public partial class UsersTabPage : Page
    {
        private readonly string _photosDirectory;

        public UsersTabPage()
        {
            InitializeComponent();
            _photosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UserPhotos");
            CreatePhotosDirectory();
            LoadUsers();
            IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void CreatePhotosDirectory()
        {
            try
            {
                if (!Directory.Exists(_photosDirectory))
                {
                    Directory.CreateDirectory(_photosDirectory);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка создания директории для фото: {ex.Message}");
            }
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    var users = db.User.ToList();
                    DataGridUser.ItemsSource = users;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                LoadUsers();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var selectedUsers = DataGridUser.SelectedItems.Cast<User>().ToList();

            if (selectedUsers.Count == 0)
            {
                MessageBox.Show("Выберите пользователей для удаления!");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {selectedUsers.Count} элементов?",
                              "Внимание",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Khakimova_DB_PaymentEntities())
                    {
                        foreach (var user in selectedUsers)
                        {
                            var userToDelete = db.User.FirstOrDefault(u => u.ID == user.ID);
                            if (userToDelete != null)
                            {
                                // Удаляем файл фото, если он существует
                                if (!string.IsNullOrEmpty(userToDelete.Photo))
                                {
                                    string photoPath = Path.Combine(_photosDirectory, userToDelete.Photo);
                                    if (File.Exists(photoPath))
                                    {
                                        File.Delete(photoPath);
                                    }
                                }

                                db.User.Remove(userToDelete);
                            }
                        }
                        db.SaveChanges();
                        MessageBox.Show("Данные успешно удалены!");
                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            if ((sender as Button)?.DataContext is User user)
            {
                NavigationService?.Navigate(new AddUserPage(user));
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
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

    // Конвертер для преобразования имени файла в изображение
    public class StringToImageConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string photoFileName && !string.IsNullOrEmpty(photoFileName))
            {
                try
                {
                    string photosDirectory = Path.Combine(Directory.GetCurrentDirectory(), "UserPhotos");
                    string photoPath = Path.Combine(photosDirectory, photoFileName);

                    if (File.Exists(photoPath))
                    {
                        var image = new BitmapImage();
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.UriSource = new Uri(photoPath);
                        image.EndInit();
                        return image;
                    }
                }
                catch
                {
                    return null;
                }
            }
            return null;
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}