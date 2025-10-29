using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class AddCategoryPage : Page
    {
        private readonly Category _currentCategory;
        private readonly Action<Category> _saveCallback;

        public AddCategoryPage(Category selectedCategory, Action<Category> saveCallback)
        {
            InitializeComponent();
            _saveCallback = saveCallback;

            if (selectedCategory != null)
            {
                _currentCategory = selectedCategory;
                TbCategoryName.Text = _currentCategory.Name;
                this.Title = "Редактирование категории";
            }
            else
            {
                this.Title = "Добавление категории";
            }
        }

        // Конструктор без callback для обратной совместимости
        public AddCategoryPage(object selectedCategory) : this(selectedCategory as Category, null)
        {
        }

        private void ButtonSaveCategory_Click(object sender, RoutedEventArgs e)
        {
            StringBuilder errors = new StringBuilder();

            if (string.IsNullOrWhiteSpace(TbCategoryName.Text))
                errors.AppendLine("Укажите название категории!");

            if (errors.Length > 0)
            {
                MessageBox.Show(errors.ToString());
                return;
            }

            try
            {
                Category categoryToSave;

                if (_currentCategory == null)
                {
                    // Создание новой категории
                    categoryToSave = new Category
                    {
                        ID = 0, // Временный ID, будет заменен в SaveCategory
                        Name = TbCategoryName.Text.Trim()
                    };
                }
                else
                {
                    // Обновление существующей категории
                    _currentCategory.Name = TbCategoryName.Text.Trim();
                    categoryToSave = _currentCategory;
                }

                // Вызываем callback для сохранения
                if (_saveCallback != null)
                {
                    _saveCallback(categoryToSave);
                    MessageBox.Show(_currentCategory == null ? "Категория успешно создана!" : "Категория успешно обновлена!");
                    NavigationService?.GoBack();
                }
                else
                {
                    MessageBox.Show("Ошибка: функция сохранения не задана");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private void ButtonClean_Click(object sender, RoutedEventArgs e)
        {
            TbCategoryName.Text = "";
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.GoBack();
        }
    }
}