using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class UsersTabPage : Page
    {
        public UsersTabPage()
        {
            InitializeComponent();
            LoadUsers();
            this.IsVisibleChanged += Page_IsVisibleChanged;
        }

        private void LoadUsers()
        {
            try
            {
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    DataGridUser.ItemsSource = db.User.ToList();
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки пользователей: {ex.Message}");
            }
        }

        private void Page_IsVisibleChanged(object sender, DependencyPropertyChangedEventArgs e)
        {
            if (Visibility == Visibility.Visible)
            {
                try
                {
                    using (var db = new Khakimova_DB_PaymentEntities())
                    {
                        db.ChangeTracker.Entries().ToList().ForEach(x => x.Reload());
                        DataGridUser.ItemsSource = db.User.ToList();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления данных: {ex.Message}");
                }
            }
        }

        private void ButtonAdd_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddUserPage(null));
        }

        private void ButtonDel_Click(object sender, RoutedEventArgs e)
        {
            var usersForRemoving = DataGridUser.SelectedItems.Cast<User>().ToList();

            if (usersForRemoving.Count == 0)
            {
                MessageBox.Show("Выберите пользователей для удаления!");
                return;
            }

            if (MessageBox.Show($"Вы точно хотите удалить {usersForRemoving.Count} элементов?",
                              "Внимание",
                              MessageBoxButton.YesNo,
                              MessageBoxImage.Question) == MessageBoxResult.Yes)
            {
                try
                {
                    using (var db = new Khakimova_DB_PaymentEntities())
                    {
                        foreach (var user in usersForRemoving)
                        {
                            var userToDelete = db.User.FirstOrDefault(u => u.ID == user.ID);
                            if (userToDelete != null)
                            {
                                db.User.Remove(userToDelete);
                            }
                        }
                        db.SaveChanges();
                        MessageBox.Show("Данные успешно удалены!");
                        LoadUsers();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка удаления: {ex.Message}");
                }
            }
        }

        private void ButtonEdit_Click(object sender, RoutedEventArgs e)
        {
            var user = (sender as Button)?.DataContext as User;
            if (user != null)
            {
                NavigationService?.Navigate(new AddUserPage(user));
            }
        }
    }
}