using System;
using System.Collections.Generic;
using System.Configuration;
using System.Data;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace Khakimova
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);

            // Загружаем тему по умолчанию при запуске
            LoadDefaultTheme();
        }

        private void LoadDefaultTheme()
        {
            try
            {
                // Загружаем светлую тему по умолчанию
                Uri uri = new Uri("DictionaryLight.xaml", UriKind.Relative);
                ResourceDictionary resourceDict = LoadComponent(uri) as ResourceDictionary;

                Current.Resources.MergedDictionaries.Clear();
                Current.Resources.MergedDictionaries.Add(resourceDict);
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки темы: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }
    }
}