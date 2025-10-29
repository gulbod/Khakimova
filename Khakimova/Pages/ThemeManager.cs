using System;
using System.IO;
using System.Windows;

namespace Khakimova
{
    public static class ThemeManager
    {
        private static string _currentTheme = "Light";

        public static string CurrentTheme
        {
            get => _currentTheme;
            set
            {
                _currentTheme = value;
                SaveThemeSetting(value);
            }
        }

        public static void ApplyTheme(string themeFile)
        {
            try
            {
                Uri uri = new Uri(themeFile, UriKind.Relative);
                ResourceDictionary themeDict = Application.LoadComponent(uri) as ResourceDictionary;

                // Очищаем существующие ресурсы
                Application.Current.Resources.MergedDictionaries.Clear();

                // Добавляем новую тему
                Application.Current.Resources.MergedDictionaries.Add(themeDict);

                // Сохраняем настройки
                SaveThemeSetting(themeFile.Contains("Dark") ? "Dark" : "Light");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка применения темы: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private static void SaveThemeSetting(string theme)
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.config");
                File.WriteAllText(filePath, theme);
            }
            catch (Exception ex)
            {
                // Игнорируем ошибки записи в файл
                System.Diagnostics.Debug.WriteLine($"Ошибка сохранения темы: {ex.Message}");
            }
        }

        public static string LoadThemeSetting()
        {
            try
            {
                string filePath = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, "theme.config");
                if (File.Exists(filePath))
                {
                    return File.ReadAllText(filePath).Trim();
                }
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine($"Ошибка загрузки темы: {ex.Message}");
            }

            return "Light"; // Значение по умолчанию
        }
    }
}