using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Data.Entity;

namespace Khakimova.Pages
{
    public partial class RegPage : Page
    {
        public RegPage()
        {
            InitializeComponent();
            ComboBoxRole.SelectedIndex = 0;
        }

        // Метод хеширования пароля
        public static string GetHash(string password)
        {
            using (SHA1 hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        // Обработчики для placeholder'ов
        private void LabelFullNameHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxFullName.Focus();
        }

        private void LabelLoginHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void LabelPasswordHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBoxFirst.Focus();
        }

        private void LabelPasswordSecondHint_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBoxSecond.Focus();
        }

        // Обработчики изменения текста
        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            LabelLoginHint.Visibility = TextBoxLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void TextBoxFullName_TextChanged(object sender, TextChangedEventArgs e)
        {
            LabelFullNameHint.Visibility = TextBoxFullName.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PasswordBoxFirst_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LabelPasswordHint.Visibility = PasswordBoxFirst.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PasswordBoxSecond_PasswordChanged(object sender, RoutedEventArgs e)
        {
            LabelPasswordSecondHint.Visibility = PasswordBoxSecond.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        // Обработчик кнопки регистрации
        private void RegisterButton_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (string.IsNullOrEmpty(TextBoxLogin.Text) ||
                string.IsNullOrEmpty(TextBoxFullName.Text) ||
                string.IsNullOrEmpty(PasswordBoxFirst.Password) ||
                string.IsNullOrEmpty(PasswordBoxSecond.Password))
            {
                MessageBox.Show("Заполните все поля!");
                return;
            }

            // Проверка формата пароля
            if (PasswordBoxFirst.Password.Length < 6)
            {
                MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                return;
            }

            bool isEnglish = true;
            bool hasNumber = false;

            // Традиционная проверка символов для C# 7.3
            foreach (char c in PasswordBoxFirst.Password)
            {
                // Проверка цифр
                if (c >= '0' && c <= '9')
                {
                    hasNumber = true;
                }
                // Проверка английских букв
                else if (!((c >= 'A' && c <= 'Z') || (c >= 'a' && c <= 'z')))
                {
                    isEnglish = false;
                }
            }

            if (!isEnglish)
            {
                MessageBox.Show("Используйте только английскую раскладку!");
                return;
            }
            else if (!hasNumber)
            {
                MessageBox.Show("Добавьте хотя бы одну цифру!");
                return;
            }

            // Проверка совпадения паролей
            if (PasswordBoxFirst.Password != PasswordBoxSecond.Password)
            {
                MessageBox.Show("Пароли не совпадают!");
                return;
            }

            // Если все проверки пройдены - регистрируем пользователя
            try
            {
                // Хеширование пароля перед сохранением
                string hashedPassword = GetHash(PasswordBoxFirst.Password);

                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    // Проверка существования логина
                    var existingUser = db.User
                        .AsNoTracking()
                        .FirstOrDefault(u => u.Login == TextBoxLogin.Text);

                    if (existingUser != null)
                    {
                        MessageBox.Show("Пользователь с таким логином уже существует!");
                        return;
                    }

                    string selectedRole = "User";
                    var selectedItem = ComboBoxRole.SelectedItem as ComboBoxItem;
                    if (selectedItem != null)
                    {
                        selectedRole = selectedItem.Content.ToString();
                    }

                    User userObject = new User
                    {
                        FIO = TextBoxFullName.Text,
                        Login = TextBoxLogin.Text,
                        Password = hashedPassword,
                        Role = selectedRole
                    };

                    db.User.Add(userObject);
                    db.SaveChanges();

                    MessageBox.Show("Пользователь успешно зарегистрирован!");

                    // Очистка полей после успешной регистрации
                    TextBoxLogin.Clear();
                    PasswordBoxFirst.Clear();
                    PasswordBoxSecond.Clear();
                    ComboBoxRole.SelectedIndex = 0;
                    TextBoxFullName.Clear();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при регистрации: {ex.Message}");
            }
        }

        // Обработчик кнопки "Назад"
        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на предыдущую страницу
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                // Если нет истории навигации, переходим на главную страницу
                NavigationService.Navigate(new AuthPage());
            }
        }
    }
}