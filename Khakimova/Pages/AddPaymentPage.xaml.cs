using System;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Khakimova.Pages
{
    public partial class AddPaymentPage : Page
    {
        private Payment _editingPayment;
        private bool _isEditMode = false;

        public AddPaymentPage(Payment paymentToEdit = null)
        {
            InitializeComponent();

            if (paymentToEdit != null)
            {
                _editingPayment = paymentToEdit;
                _isEditMode = true;
                PageTitle.Text = "Редактирование платежа";
                LoadPaymentData();
            }

            InitializeControls();
        }

        private void InitializeControls()
        {
            // Заполняем категории из DataManager.Categories
            CategoryComboBox.Items.Clear();
            if (DataManager.Categories != null && DataManager.Categories.Any())
            {
                foreach (var category in DataManager.Categories)
                {
                    CategoryComboBox.Items.Add(category.Name);
                }
            }
            else
            {
                // Запасной вариант, если категории не загружены
                CategoryComboBox.Items.Add("Коммунальные платежи");
                CategoryComboBox.Items.Add("Интернет и связь");
                CategoryComboBox.Items.Add("Транспорт");
                CategoryComboBox.Items.Add("Продукты питания");
            }

            // Заполняем статусы
            StatusComboBox.Items.Clear();
            StatusComboBox.Items.Add("Оплачен");
            StatusComboBox.Items.Add("В обработке");
            StatusComboBox.Items.Add("Ожидает оплаты");
            StatusComboBox.Items.Add("Отменен");

            // Заполняем пользователей (можно заменить на реальных пользователей из БД)
            UserNameComboBox.Items.Clear();
            UserNameComboBox.Items.Add("Иванов И.И.");
            UserNameComboBox.Items.Add("Петров П.П.");
            UserNameComboBox.Items.Add("Сидорова А.С.");
            UserNameComboBox.Items.Add("Ахметова М.З.");

            // Устанавливаем текущую дату по умолчанию
            if (!_isEditMode)
            {
                DatePicker.SelectedDate = DateTime.Now;
                StatusComboBox.SelectedItem = "В обработке";
            }
        }

        private void LoadPaymentData()
        {
            if (_editingPayment != null)
            {
                UserNameComboBox.SelectedItem = _editingPayment.UserName;
                CategoryComboBox.SelectedItem = _editingPayment.Category;
                AmountTextBox.Text = _editingPayment.Amount.ToString("F2", CultureInfo.InvariantCulture);
                DatePicker.SelectedDate = _editingPayment.Date;
                StatusComboBox.SelectedItem = _editingPayment.Status;
            }
        }

        private void SaveButton_Click(object sender, RoutedEventArgs e)
        {
            if (!ValidateInput())
                return;

            try
            {
                if (_isEditMode && _editingPayment != null)
                {
                    // Редактирование существующего платежа
                    _editingPayment.UserName = UserNameComboBox.SelectedItem.ToString();
                    _editingPayment.Category = CategoryComboBox.SelectedItem.ToString();
                    _editingPayment.Amount = decimal.Parse(AmountTextBox.Text);
                    _editingPayment.Date = DatePicker.SelectedDate ?? DateTime.Now;
                    _editingPayment.Status = StatusComboBox.SelectedItem.ToString();

                    MessageBox.Show("Платеж успешно обновлен!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }
                else
                {
                    // Добавление нового платежа
                    var newPayment = new Payment
                    {
                        ID = PaymentDataManager.GetNextId(),
                        UserName = UserNameComboBox.SelectedItem.ToString(),
                        Category = CategoryComboBox.SelectedItem.ToString(),
                        Amount = decimal.Parse(AmountTextBox.Text),
                        Date = DatePicker.SelectedDate ?? DateTime.Now,
                        Status = StatusComboBox.SelectedItem.ToString()
                    };

                    PaymentDataManager.Payments.Add(newPayment);
                    MessageBox.Show("Платеж успешно добавлен!", "Успех",
                                  MessageBoxButton.OK, MessageBoxImage.Information);
                }

                // Возвращаемся на предыдущую страницу
                if (NavigationService.CanGoBack)
                    NavigationService.GoBack();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка сохранения: {ex.Message}", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Error);
            }
        }

        private void CancelButton_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService.CanGoBack)
                NavigationService.GoBack();
        }

        private bool ValidateInput()
        {
            // Проверка пользователя
            if (UserNameComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите пользователя!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                UserNameComboBox.Focus();
                return false;
            }

            // Проверка категории
            if (CategoryComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите категорию!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                CategoryComboBox.Focus();
                return false;
            }

            // Проверка суммы
            if (string.IsNullOrWhiteSpace(AmountTextBox.Text) ||
                !decimal.TryParse(AmountTextBox.Text, out decimal amount) ||
                amount <= 0)
            {
                MessageBox.Show("Введите корректную сумму (больше 0)!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                AmountTextBox.Focus();
                return false;
            }

            // Проверка даты
            if (DatePicker.SelectedDate == null)
            {
                MessageBox.Show("Выберите дату!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                DatePicker.Focus();
                return false;
            }

            // Проверка статуса
            if (StatusComboBox.SelectedItem == null)
            {
                MessageBox.Show("Выберите статус!", "Ошибка",
                              MessageBoxButton.OK, MessageBoxImage.Warning);
                StatusComboBox.Focus();
                return false;
            }

            return true;
        }

        private void AmountTextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            // Разрешаем только цифры и запятую/точку для десятичных чисел
            var textBox = sender as TextBox;

            // Проверяем, является ли ввод числом (включая десятичные разделители)
            if (!Regex.IsMatch(e.Text, @"^[0-9.,]$"))
            {
                e.Handled = true;
                return;
            }

            // Проверяем, что есть только один десятичный разделитель
            if ((e.Text == "," || e.Text == ".") && textBox.Text.ContainsAny(',', '.'))
            {
                e.Handled = true;
                return;
            }

            // Проверяем, что после разделителя не более 2 цифр
            if (textBox.Text.ContainsAny(',', '.') &&
                textBox.Text.Split(',', '.')[1].Length >= 2)
            {
                e.Handled = true;
            }
        }
    }

    // Extension метод для проверки наличия символов в строке
    public static class StringExtensions
    {
        public static bool ContainsAny(this string str, params char[] chars)
        {
            if (string.IsNullOrEmpty(str)) return false;

            foreach (char c in chars)
            {
                if (str.Contains(c))
                    return true;
            }
            return false;
        }
    }
}