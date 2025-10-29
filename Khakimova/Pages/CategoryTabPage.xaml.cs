using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class CategoryTabPage : Page
    {
        public CategoryTabPage()
        {
            InitializeComponent();
            LoadCategories();
        }

        private void LoadCategories()
        {
            try
            {
                // Используем статическую коллекцию из DataManager
                DataGridCategory.ItemsSource = DataManager.Categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            // Передаем функцию сохранения
            NavigationService?.Navigate(new AddCategoryPage(null, SaveCategory));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            if (DataGridCategory.SelectedItem is Category selectedCategory)
            {
                string categoryName = selectedCategory.Name;

                if (MessageBox.Show($"Вы точно хотите удалить категорию \"{categoryName}\"?",
                                  "Внимание",
                                  MessageBoxButton.YesNo,
                                  MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        DataManager.Categories.Remove(selectedCategory);
                        MessageBox.Show("Категория успешно удалена!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
            else
            {
                MessageBox.Show("Выберите категорию для удаления!");
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Category selectedCategory)
            {
                // Передаем функцию сохранения
                NavigationService?.Navigate(new AddCategoryPage(selectedCategory, SaveCategory));
            }
        }

        private void SaveCategory(Category category)
        {
            try
            {
                if (category.ID == 0) // Новая категория
                {
                    category.ID = DataManager.GetNextId();
                    DataManager.Categories.Add(category);
                }
                else // Редактирование существующей
                {
                    var existingCategory = DataManager.Categories.FirstOrDefault(c => c.ID == category.ID);
                    if (existingCategory != null)
                    {
                        existingCategory.Name = category.Name;
                    }
                }

                // Принудительно обновляем DataGrid
                DataGridCategory.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения категории: {ex.Message}");
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Нет предыдущей страницы для возврата");
            }
        }
    }
}