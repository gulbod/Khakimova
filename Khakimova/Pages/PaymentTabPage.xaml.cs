using System;
using System.Linq;
using System.Windows;
using System.Windows.Controls;

namespace Khakimova.Pages
{
    public partial class PaymentTabPage : Page
    {
        public PaymentTabPage()
        {
            InitializeComponent();
            LoadPayments();
        }

        private void LoadPayments()
        {
            try
            {
                // Заглушка для демонстрации
                var payments = new[]
                {
                    new { ID = 1, UserName = "Иванов И.И.", Category = "Коммунальные", Amount = 2500.50m, Date = DateTime.Now.AddDays(-5), Status = "Оплачен" },
                    new { ID = 2, UserName = "Петров П.П.", Category = "Интернет", Amount = 500.00m, Date = DateTime.Now.AddDays(-3), Status = "Оплачен" },
                    new { ID = 3, UserName = "Сидорова А.С.", Category = "Транспорт", Amount = 1500.00m, Date = DateTime.Now.AddDays(-1), Status = "В обработке" }
                };

                PaymentsDataGrid.ItemsSource = payments;
            }
            catch (System.Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}");
            }
        }

        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            MessageBox.Show("Функция добавления платежа", "Добавление платежа");
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }
    }
}