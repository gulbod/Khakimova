using System;
using System.Text;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class AddCategoryPage : Page
    {
        private readonly Category _currentCategory;

        public AddCategoryPage(object selectedCategory)
        {
            InitializeComponent();

            if (selectedCategory is Category category)
            {
                _currentCategory = category;
                // Установка данных из selectedCategory
                TbCategoryName.Text = _currentCategory.Name;

                // Меняем заголовок для режима редактирования
                this.Title = "Редактирование категории";
            }
            else
            {
                this.Title = "Добавление категории";
            }
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
                if (_currentCategory == null)
                {
                    // Логика создания новой категории
                    var newCategory = new Category
                    {
                        ID = GenerateNewId(), // Нужно генерировать новый ID
                        Name = TbCategoryName.Text
                    };

                    // В реальном приложении здесь будет сохранение в БД
                    // _context.Categories.Add(newCategory);
                    // _context.SaveChanges();

                    MessageBox.Show("Категория успешно создана!");
                }
                else
                {
                    // Логика обновления существующей категории
                    _currentCategory.Name = TbCategoryName.Text;

                    // В реальном приложении здесь будет обновление в БД
                    // _context.SaveChanges();

                    MessageBox.Show("Категория успешно обновлена!");
                }

                NavigationService?.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}");
            }
        }

        private int GenerateNewId()
        {
            // В реальном приложении ID генерируется базой данных
            // Здесь простая заглушка для демонстрации
            return DateTime.Now.Second + DateTime.Now.Millisecond;
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