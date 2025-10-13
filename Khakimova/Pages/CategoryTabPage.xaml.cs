using System;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class CategoryTabPage : Page
    {
        private ObservableCollection<Category> _categories;

        public CategoryTabPage()
        {
            InitializeComponent();
            _categories = new ObservableCollection<Category>();
            LoadCategories();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadCategories()
        {
            try
            {
                _categories.Clear();

                // Заглушка для демонстрации - в реальном приложении здесь будет загрузка из БД
                var demoCategories = new[]
                {
                    new Category { ID = 1, Name = "Коммунальные платежи" },
                    new Category { ID = 2, Name = "Интернет и связь" },
                    new Category { ID = 3, Name = "Транспорт" },
                    new Category { ID = 4, Name = "Продукты питания" }
                };

                foreach (var category in demoCategories)
                {
                    _categories.Add(category);
                }

                DataGridCategory.ItemsSource = _categories;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки категорий: {ex.Message}");
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                LoadCategories();
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            // Переход на страницу добавления с null - означает создание новой категории
            NavigationService?.Navigate(new AddCategoryPage(null));
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
                        // Удаление из коллекции
                        _categories.Remove(selectedCategory);

                        // В реальном приложении здесь будет вызов метода для удаления из базы данных
                        // _context.Categories.Remove(selectedCategory);
                        // _context.SaveChanges();

                        MessageBox.Show("Категория успешно удалена!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                        LoadCategories(); // Перезагружаем в случае ошибки
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
                // Передача выбранной категории на страницу редактирования
                NavigationService?.Navigate(new AddCategoryPage(selectedCategory));
            }
        }

        private void ButtonBack_Click(object sender, RoutedEventArgs e)
        {
            // Возврат на предыдущую страницу
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