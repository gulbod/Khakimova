using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class ChangePassPage : Page
    {
        public ChangePassPage()
        {
            InitializeComponent();
        }

        // Метод хеширования пароля (такой же как в AuthPage и RegPage)
        public static string GetHash(string password)
        {
            using (SHA1 hash = SHA1.Create())
            {
                return string.Concat(hash.ComputeHash(Encoding.UTF8.GetBytes(password))
                    .Select(x => x.ToString("X2")));
            }
        }

        private void ButtonSave_Click(object sender, RoutedEventArgs e)
        {
            // Проверка заполнения всех полей
            if (string.IsNullOrEmpty(CurrentPasswordBox.Password) ||
                string.IsNullOrEmpty(NewPasswordBox.Password) ||
                string.IsNullOrEmpty(ConfirmPasswordBox.Password) ||
                string.IsNullOrEmpty(TbLogin.Text))
            {
                MessageBox.Show("Все поля обязательны к заполнению!");
                return;
            }

            // Проверка правильности текущих данных аккаунта
            string hashedCurrentPass = GetHash(CurrentPasswordBox.Password);

            using (var db = new Khakimova_DB_PaymentEntities())
            {
                var user = db.User
                    .FirstOrDefault(u => u.Login == TbLogin.Text && u.Password == hashedCurrentPass);

                if (user == null)
                {
                    MessageBox.Show("Текущий пароль/Логин неверный!");
                    return;
                }

                // Проверка формата нового пароля
                if (NewPasswordBox.Password.Length < 6)
                {
                    MessageBox.Show("Пароль слишком короткий, должно быть минимум 6 символов!");
                    return;
                }

                bool isEnglish = true;
                bool hasNumber = false;

                // Проверка символов нового пароля
                foreach (char c in NewPasswordBox.Password)
                {
                    if (c >= '0' && c <= '9')
                    {
                        hasNumber = true;
                    }
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

                // Проверка совпадения новых паролей
                if (NewPasswordBox.Password != ConfirmPasswordBox.Password)
                {
                    MessageBox.Show("Новые пароли не совпадают!");
                    return;
                }

                // Если все проверки пройдены - сохраняем новый пароль
                try
                {
                    string hashedNewPassword = GetHash(NewPasswordBox.Password);
                    user.Password = hashedNewPassword;
                    db.SaveChanges();

                    MessageBox.Show("Пароль успешно изменен!");
                    NavigationService?.Navigate(new AuthPage());
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка при изменении пароля: {ex.Message}");
                }
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AuthPage());
        }
    }
}