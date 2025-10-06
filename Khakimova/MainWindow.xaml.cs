using Khakimova.Pages;
using System;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Navigation;

namespace Khakimova
{
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ThemeRadioButton_Checked(object sender, RoutedEventArgs e)
        {
            RadioButton radioButton = sender as RadioButton;
            if (radioButton == null || !radioButton.IsChecked.Value)
                return;

            string themeFile;

            if (radioButton == LightThemeRadio)
            {
                themeFile = "DictionaryLight.xaml"; // Светлая тема
            }
            else
            {
                themeFile = "DictionaryDark.xaml"; // Темная тема
            }

            // Загружаем выбранный словарь стилей
            ChangeTheme(themeFile);
        }

        private void ChangeTheme(string themeFile)
        {
            try
            {
                // Определяем путь к файлу ресурсов
                Uri uri = new Uri(themeFile, UriKind.Relative);

                // Загружаем словарь ресурсов
                ResourceDictionary resourceDict = Application.LoadComponent(uri) as ResourceDictionary;

                // Очищаем текущие ресурсы
                Application.Current.Resources.MergedDictionaries.Clear();

                // Добавляем загруженный словарь ресурсов
                Application.Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки темы: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void ToAuthPage_btn(object sender, RoutedEventArgs e)
        {
            Main.Content = new AuthPage();
        }

        private void Main_Navigated(object sender, NavigationEventArgs e)
        {

        }
    }
}