using System;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Khakimova.Pages
{
    public partial class AuthPage : Page
    {
        private readonly int _maxFailedAttempts = 3;
        private int _failedAttempts = 0;

        public AuthPage()
        {
            InitializeComponent();
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

        private void ButtonEnter_OnClick(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(TextBoxLogin.Text) || string.IsNullOrEmpty(PasswordBox.Password))
            {
                MessageBox.Show("Введите логин и пароль");
                return;
            }

            // Хеширование введенного пароля перед сравнением с БД
            string hashedPassword = GetHash(PasswordBox.Password);

            using (var db = new Khakimova_DB_PaymentEntities())
            {
                var user = db.User
                    .AsNoTracking()
                    .FirstOrDefault(u => u.Login == TextBoxLogin.Text && u.Password == hashedPassword);

                if (user == null)
                {
                    MessageBox.Show("Пользователь с такими данными не найден!");
                    _failedAttempts++;

                    if (_failedAttempts >= _maxFailedAttempts)
                    {
                        if (CaptchaContainer.Visibility != Visibility.Visible)
                        {
                            CaptchaSwitch();
                        }
                        CaptchaChange();
                    }
                    return;
                }

                MessageBox.Show("Пользователь успешно найден!");

                // Используем традиционный switch для C# 7.3
                switch (user.Role)
                {
                    case "User":
                        NavigationService?.Navigate(new UserPage());
                        break;
                    case "Admin":
                        NavigationService?.Navigate(new AdminPage());
                        break;
                    default:
                        MessageBox.Show("Неизвестная роль пользователя!");
                        break;
                }
            }
        }

        private void CaptchaSwitch()
        {
            if (CaptchaContainer.Visibility == Visibility.Visible)
            {
                // Скрываем капчу
                TextBoxLogin.Clear();
                PasswordBox.Clear();
                CaptchaContainer.Visibility = Visibility.Collapsed;
            }
            else
            {
                // Показываем капчу
                CaptchaContainer.Visibility = Visibility.Visible;
            }
        }

        private void CaptchaChange()
        {
            string allowedCharacters = "A,B,C,D,E,F,G,H,I,J,K,L,M,N,O,P,Q,R,S,T,U,V,W,X,Y,Z," +
                                      "a,b,c,d,e,f,g,h,i,j,k,l,m,n,o,p,q,r,s,t,u,v,w,y,z," +
                                      "1,2,3,4,5,6,7,8,9,0";

            string[] characters = allowedCharacters.Split(',');
            string captchaText = "";

            Random random = new Random();

            for (int i = 0; i < 6; i++)
            {
                string randomChar = characters[random.Next(0, characters.Length)];
                captchaText += randomChar;
            }
            CaptchaText.Text = captchaText;
            CaptchaInput.Clear();
        }

        private void SubmitCaptchaButton_Click(object sender, RoutedEventArgs e)
        {
            if (CaptchaInput.Text != CaptchaText.Text)
            {
                MessageBox.Show("Неверно введена капча", "Ошибка");
                CaptchaChange();
            }
            else
            {
                MessageBox.Show("Капча введена успешно, можете продолжить авторизацию", "Успех");
                CaptchaSwitch();
                _failedAttempts = 0;
            }
        }

        private void ButtonReg_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new RegPage());
        }

        private void ButtonChangePassword_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new ChangePassPage());
        }

        private void TextHintLogin_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            TextBoxLogin.Focus();
        }

        private void TextHintPassword_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            PasswordBox.Focus();
        }

        private void TextBoxLogin_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextHintLogin.Visibility = TextBoxLogin.Text.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }

        private void PasswordBox_PasswordChanged(object sender, RoutedEventArgs e)
        {
            TextHintPassword.Visibility = PasswordBox.Password.Length > 0 ? Visibility.Hidden : Visibility.Visible;
        }
    }
}