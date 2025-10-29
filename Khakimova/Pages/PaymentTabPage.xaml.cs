using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
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
                PaymentsDataGrid.ItemsSource = null;
                PaymentsDataGrid.ItemsSource = PaymentDataManager.Payments;
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки платежей: {ex.Message}");
            }
        }

        private void AddPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AddPaymentPage(null));
        }

        private void EditPaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Payment selectedPayment)
            {
                NavigationService?.Navigate(new AddPaymentPage(selectedPayment));
            }
        }

        private void DeletePaymentButton_Click(object sender, RoutedEventArgs e)
        {
            var button = sender as Button;
            if (button?.DataContext is Payment selectedPayment)
            {
                string paymentInfo = $"Платеж #{selectedPayment.ID} - {selectedPayment.UserName} ({selectedPayment.Amount} руб.)";

                if (MessageBox.Show($"Вы точно хотите удалить {paymentInfo}?",
                                  "Внимание",
                                  MessageBoxButton.YesNo,
                                  MessageBoxImage.Question) == MessageBoxResult.Yes)
                {
                    try
                    {
                        PaymentDataManager.Payments.Remove(selectedPayment);
                        MessageBox.Show("Платеж успешно удален!");
                    }
                    catch (Exception ex)
                    {
                        MessageBox.Show($"Ошибка удаления: {ex.Message}");
                    }
                }
            }
        }

        private void RefreshButton_Click(object sender, RoutedEventArgs e)
        {
            LoadPayments();
        }

        private void BackButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
            {
                NavigationService.GoBack();
            }
            else
            {
                MessageBox.Show("Переход на AdminPage - замените на вашу реализацию");
            }
        }
    }

    // Класс модели платежа
    public class Payment : INotifyPropertyChanged
    {
        private int _id;
        private string _userName;
        private string _category;
        private decimal _amount;
        private DateTime _date;
        private string _status;

        public int ID
        {
            get => _id;
            set
            {
                _id = value;
                OnPropertyChanged(nameof(ID));
            }
        }

        public string UserName
        {
            get => _userName;
            set
            {
                _userName = value;
                OnPropertyChanged(nameof(UserName));
            }
        }

        public string Category
        {
            get => _category;
            set
            {
                _category = value;
                OnPropertyChanged(nameof(Category));
            }
        }

        public decimal Amount
        {
            get => _amount;
            set
            {
                _amount = value;
                OnPropertyChanged(nameof(Amount));
            }
        }

        public DateTime Date
        {
            get => _date;
            set
            {
                _date = value;
                OnPropertyChanged(nameof(Date));
            }
        }

        public string Status
        {
            get => _status;
            set
            {
                _status = value;
                OnPropertyChanged(nameof(Status));
            }
        }

        public event PropertyChangedEventHandler PropertyChanged;

        protected virtual void OnPropertyChanged(string propertyName)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }

    // Статический класс для управления платежами
    public static class PaymentDataManager
    {
        public static ObservableCollection<Payment> Payments { get; } = new ObservableCollection<Payment>
        {
            new Payment { ID = 1, UserName = "Иванов И.И.", Category = "Коммунальные платежи", Amount = 2500.50m, Date = DateTime.Now.AddDays(-5), Status = "Оплачен" },
            new Payment { ID = 2, UserName = "Петров П.П.", Category = "Интернет и связь", Amount = 500.00m, Date = DateTime.Now.AddDays(-3), Status = "Оплачен" },
            new Payment { ID = 3, UserName = "Сидорова А.С.", Category = "Транспорт", Amount = 1500.00m, Date = DateTime.Now.AddDays(-1), Status = "В обработке" }
        };

        public static int GetNextId()
        {
            return Payments.Any() ? Payments.Max(p => p.ID) + 1 : 1;
        }
    }
}