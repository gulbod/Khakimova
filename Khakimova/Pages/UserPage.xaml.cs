using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class UserPage : Page
    {
        public UserPage()
        {
            InitializeComponent();
            LoadUsers();
        }

        // Метод загрузки пользователей
        private void LoadUsers()
        {
            try
            {
                // Временные данные для демонстрации (без БД)
                var currentUsers = GetDemoUsers();
                ListUser.ItemsSource = currentUsers;

                // Раскомментировать при подключении реальной БД
                /*
                var currentUsers = Entities.GetContext().Users.ToList();
                ListUser.ItemsSource = currentUsers;
                */
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при загрузке пользователей: {ex.Message}");
            }
        }

        // Временный метод для демонстрации (без БД)
        private List<Users> GetDemoUsers()
        {
            return new List<Users>
            {
                new Users { Id = 1, FIO = "Иванов Иван Иванович", Login = "ivanov", Role = "User" },
                new Users { Id = 2, FIO = "Петров Петр Петрович", Login = "petrov", Role = "User" },
                new Users { Id = 3, FIO = "Сидорова Анна Сергеевна", Login = "sidorova", Role = "User" },
                new Users { Id = 4, FIO = "Козлов Алексей Владимирович", Login = "kozlov", Role = "Admin" },
                new Users { Id = 5, FIO = "Смирнова Мария Дмитриевна", Login = "smirnova", Role = "User" },
                new Users { Id = 6, FIO = "Васильев Дмитрий Олегович", Login = "vasilev", Role = "Admin" },
                new Users { Id = 7, FIO = "Николаева Екатерина Игоревна", Login = "nikolaeva", Role = "User" },
                new Users { Id = 8, FIO = "Федоров Сергей Александрович", Login = "fedorov", Role = "User" }
            };
        }

        // Очистка фильтров
        private void clearFiltersButton_Click_1(object sender, RoutedEventArgs e)
        {
            fioFilterTextBox.Text = "";
            sortComboBox.SelectedIndex = 0;
            onlyAdminCheckBox.IsChecked = false;
        }

        // Обработчики фильтров
        private void fioFilterTextBox_TextChanged(object sender, TextChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void sortComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Checked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        private void onlyAdminCheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            UpdateUsers();
        }

        // Обновление списка пользователей с учетом фильтров
        private void UpdateUsers()
        {
            if (!IsInitialized)
            {
                return;
            }

            try
            {
                // Временные данные (без БД)
                List<Users> currentUsers = GetDemoUsers();

                // Раскомментировать при подключении реальной БД
                /*
                List<Users> currentUsers = Entities.GetContext().Users.ToList();
                */

                // Фильтрация по ФИО
                if (!string.IsNullOrWhiteSpace(fioFilterTextBox.Text))
                {
                    currentUsers = currentUsers.Where(x =>
                        x.FIO.ToLower().Contains(fioFilterTextBox.Text.ToLower())).ToList();
                }

                // Фильтрация по роли
                if (onlyAdminCheckBox.IsChecked.Value)
                {
                    currentUsers = currentUsers.Where(x => x.Role == "Admin").ToList();
                }

                // Сортировка по ФИО
                if (sortComboBox.SelectedIndex == 0)
                {
                    currentUsers = currentUsers.OrderBy(x => x.FIO).ToList();
                }
                else
                {
                    currentUsers = currentUsers.OrderByDescending(x => x.FIO).ToList();
                }

                ListUser.ItemsSource = currentUsers;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка при обновлении списка: {ex.Message}");
            }
        }
    }
}