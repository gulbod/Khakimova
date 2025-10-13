using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Imaging;
using System.IO;
using System.Windows.Media;

namespace Khakimova.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
        }

        // Метод загрузки пользователей из БД
        private void LoadUsers()
        {
            try
            {
                // Использовать реальную БД при подключении
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    var currentUsers = db.User.ToList();
                    ListUser.ItemsSource = currentUsers;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
                LoadDemoUsers();
            }
        }

        // Временный метод для демонстрации (без БД)
        private void LoadDemoUsers()
        {
            var demoUsers = new List<User>
            {
                new User { ID = 1, FIO = "Иванов Иван Иванович", Login = "user1", Role = "User" },
                new User { ID = 2, FIO = "Петров Петр Петрович", Login = "user2", Role = "User" },
                new User { ID = 3, FIO = "Сидорова Анна Сергеевна", Login = "user3", Role = "User" },
                new User { ID = 4, FIO = "Козлов Алексей Владимирович", Login = "admin1", Role = "Admin" },
                new User { ID = 5, FIO = "Смирнова Мария Дмитриевна", Login = "user4", Role = "User" },
                new User { ID = 6, FIO = "Васильев Дмитрий Олегович", Login = "admin2", Role = "Admin" },
                new User { ID = 7, FIO = "Николаева Екатерина Игоревна", Login = "user5", Role = "User" },
                new User { ID = 8, FIO = "Фёдоров Сергей Александрович", Login = "user6", Role = "User" }
            };

            ListUser.ItemsSource = demoUsers;
        }

        // Очистка фильтров
        private void ClearFiltersButton_Click(object sender, RoutedEventArgs e)
        {
            FioFilterTextBox.Text = "";
            SortComboBox.SelectedIndex = 0;
            OnlyAdminCheckBox.IsChecked = false;
        }

        // Обработчики фильтров
        private void FioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void SortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void OnlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void OnlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        // Обновление списка пользователей с учетом фильтров
        private void UpdateUsers()
        {
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                List<User> currentUsers;

                // Использовать реальную БД при подключении
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    currentUsers = db.User.ToList();
                }

                // Фильтрация по ФИО
                if (!string.IsNullOrWhiteSpace(FioFilterTextBox.Text))
                {
                    currentUsers = currentUsers.Where(x =>
                        x.FIO.ToLower().Contains(FioFilterTextBox.Text.ToLower())).ToList();
                }

                // Фильтрация по роли
                if (OnlyAdminCheckBox.IsChecked == true)
                {
                    currentUsers = currentUsers.Where(x => x.Role == "Admin").ToList();
                }

                // Сортировка по ФИО
                if (SortComboBox.SelectedIndex == 0)
                {
                    currentUsers = currentUsers.OrderBy(x => x.FIO).ToList();
                }
                else
                {
                    currentUsers = currentUsers.OrderByDescending(x => x.FIO).ToList();
                }

                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка: {ex.Message}");
                LoadDemoUsers();
            }
        }

        // Обработчик двойного клика по пользователю
        private void ListUser_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (ListUser.SelectedItem is User selectedUser)
            {
                // Переход на страницу редактирования при двойном клике
                NavigationService?.Navigate(new AddUserPage(selectedUser));
            }
        }

        // Кнопка редактирования пользователя
        private void EditUserButton_Click(object sender, RoutedEventArgs e)
        {
            if (ListUser.SelectedItem is User selectedUser)
            {
                // Переход на страницу редактирования пользователя
                NavigationService?.Navigate(new AddUserPage(selectedUser));
            }
            else
            {
                MessageBox.Show("Выберите пользователя для редактирования!", "Внимание",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        // Кнопка "Назад" - возврат на предыдущую страницу
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет предыдущей страницы для возврата.", "Информация",
                              MessageBoxButton.OK, MessageBoxImage.Information);

                // Если нельзя вернуться назад, можно перейти на главную страницу
                // NavigationService?.Navigate(new MainPage());
            }
        }

        // Обновление списка
        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadUsers();
            MessageBox.Show("Список пользователей обновлен!", "Обновление",
                          MessageBoxButton.OK, MessageBoxImage.Information);
        }

        // Загрузка страницы
        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            LoadUsers();
        }
    }

    // Конвертер для преобразования массива байтов в изображение
    public class ByteArrayToImageConverter : System.Windows.Data.IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is byte[] byteArray && byteArray.Length > 0)
            {
                try
                {
                    var image = new BitmapImage();
                    using (var stream = new MemoryStream(byteArray))
                    {
                        image.BeginInit();
                        image.CacheOption = BitmapCacheOption.OnLoad;
                        image.StreamSource = stream;
                        image.EndInit();
                    }
                    return image;
                }
                catch
                {
                    return GetDefaultImage();
                }
            }
            return GetDefaultImage();
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private BitmapImage GetDefaultImage()
        {
            // Возвращает изображение-заглушку
            try
            {
                return new BitmapImage(new Uri("pack://application:,,,/Images/default.png"));
            }
            catch
            {
                // Если изображение не найдено, создаем простой серый квадрат
                return CreateDefaultImage();
            }
        }

        private BitmapImage CreateDefaultImage()
        {
            int width = 100;
            int height = 100;

            var visual = new DrawingVisual();
            using (var context = visual.RenderOpen())
            {
                context.DrawRectangle(Brushes.LightGray, null, new Rect(0, 0, width, height));
                var text = new FormattedText("No Image",
                    System.Globalization.CultureInfo.CurrentCulture,
                    System.Windows.FlowDirection.LeftToRight,
                    new Typeface("Arial"), 10, Brushes.Gray);
                context.DrawText(text, new Point(width / 2 - text.Width / 2, height / 2 - text.Height / 2));
            }

            var bitmap = new RenderTargetBitmap(width, height, 96, 96, PixelFormats.Pbgra32);
            bitmap.Render(visual);

            var encoder = new PngBitmapEncoder();
            encoder.Frames.Add(BitmapFrame.Create(bitmap));

            using (var stream = new MemoryStream())
            {
                encoder.Save(stream);
                stream.Position = 0;

                var result = new BitmapImage();
                result.BeginInit();
                result.CacheOption = BitmapCacheOption.OnLoad;
                result.StreamSource = stream;
                result.EndInit();
                return result;
            }
        }
    }

    // Конвертер для цветов ролей
    public class RoleToColorConverter : System.Windows.Data.IValueConverter
    {
        public static RoleToColorConverter Instance { get; } = new RoleToColorConverter();

        public object Convert(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            if (value is string role)
            {
                return role == "Admin" ? new SolidColorBrush(Colors.Red) : new SolidColorBrush(Colors.Green);
            }
            return new SolidColorBrush(Colors.Gray);
        }

        public object ConvertBack(object value, Type targetType, object parameter, System.Globalization.CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}