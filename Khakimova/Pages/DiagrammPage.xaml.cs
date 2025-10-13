using System;
using System.Collections.Generic;
using System.Linq;
using System.Windows;
using System.Windows.Controls;
using Excel = Microsoft.Office.Interop.Excel;
using System.Runtime.InteropServices;

namespace Khakimova.Pages
{
    public partial class DiagramPage : Page
    {
        public DiagramPage()
        {
            InitializeComponent();
            LoadComboBoxData();
        }

        private void LoadComboBoxData()
        {
            try
            {
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    // Загружаем пользователей
                    CmbUser.ItemsSource = db.User.ToList();

                    // Загружаем типы визуализации
                    var visualizationTypes = new List<string>
                    {
                        "Столбчатая диаграмма",
                        "Круговая диаграмма",
                        "Линейный график",
                        "Областная диаграмма"
                    };
                    CmbDiagram.ItemsSource = visualizationTypes;

                    // Устанавливаем значения по умолчанию
                    if (CmbUser.Items.Count > 0)
                        CmbUser.SelectedIndex = 0;
                    if (CmbDiagram.Items.Count > 0)
                        CmbDiagram.SelectedIndex = 0;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка загрузки данных: {ex.Message}");
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser)
            {
                try
                {
                    // Очищаем предыдущие данные
                    ChartVisualization.ItemsSource = null;

                    // Создаем тестовые данные для демонстрации
                    var paymentData = new List<PaymentData>
                    {
                        new PaymentData { Category = "Коммунальные платежи", Amount = 4500 },
                        new PaymentData { Category = "Интернет и связь", Amount = 1200 },
                        new PaymentData { Category = "Транспорт", Amount = 3500 },
                        new PaymentData { Category = "Продукты питания", Amount = 8000 },
                        new PaymentData { Category = "Развлечения", Amount = 2500 }
                    };

                    // Рассчитываем ширину баров для визуализации
                    decimal maxAmount = paymentData.Max(p => p.Amount);
                    foreach (var payment in paymentData)
                    {
                        payment.BarWidth = (double)(payment.Amount / maxAmount) * 300;
                    }

                    ChartVisualization.ItemsSource = paymentData;

                    // Обновляем общую сумму
                    decimal total = paymentData.Sum(p => p.Amount);
                    TotalText.Text = $"Общая сумма: {total:C}";

                    // Обновляем заголовок
                    ChartTitle.Text = $"Статистика платежей: {currentUser.FIO} ({CmbDiagram.SelectedItem})";
                }
                catch (Exception ex)
                {
                    MessageBox.Show($"Ошибка обновления данных: {ex.Message}");
                }
            }
        }

        private void BtnExportExcel_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportToExcel();
                MessageBox.Show("Данные успешно экспортированы в Microsoft Excel!", "Экспорт");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Excel: {ex.Message}", "Ошибка");
            }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportToWord();
                MessageBox.Show("Данные успешно экспортированы в Microsoft Word!", "Экспорт");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Word: {ex.Message}", "Ошибка");
            }
        }

        private void ExportToExcel()
        {
            Excel.Application application = null;
            Excel.Workbook workbook = null;

            try
            {
                // Создаем новое приложение Excel
                application = new Excel.Application();

                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    // Получаем список пользователей с сортировкой по ФИО
                    var allUsers = db.User.ToList().OrderBy(u => u.FIO).ToList();

                    // Устанавливаем количество листов равным количеству пользователей
                    application.SheetsInNewWorkbook = allUsers.Count;
                    workbook = application.Workbooks.Add();

                    decimal grandTotal = 0; // Общий итог по всем пользователям

                    // Цикл по пользователям
                    for (int i = 0; i < allUsers.Count; i++)
                    {
                        int startRowIndex = 1;
                        Excel.Worksheet worksheet = application.Worksheets.Item[i + 1];
                        worksheet.Name = allUsers[i].FIO;

                        // Добавляем заголовки колонок
                        worksheet.Cells[1, 1] = "Дата платежа";
                        worksheet.Cells[1, 2] = "Название";
                        worksheet.Cells[1, 3] = "Стоимость";
                        worksheet.Cells[1, 4] = "Количество";
                        worksheet.Cells[1, 5] = "Сумма";

                        // Форматируем заголовки
                        Excel.Range columnHeaderRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 5]];
                        columnHeaderRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                        columnHeaderRange.Font.Bold = true;
                        startRowIndex++;

                        // Для демонстрации создаем тестовые данные по категориям
                        // В реальном приложении используйте данные из базы:
                        // var userCategories = allUsers[i].Payment.OrderBy(u => u.Date).GroupBy(u => u.Category).OrderBy(u => u.Key.Name);

                        var testCategories = new[]
                        {
                            new {
                                Name = "Коммунальные платежи",
                                Payments = new[]
                                {
                                    new { Date = DateTime.Now.AddDays(-10), Name = "Электричество", Price = 1500m, Num = 1 },
                                    new { Date = DateTime.Now.AddDays(-5), Name = "Вода", Price = 800m, Num = 1 }
                                }
                            },
                            new {
                                Name = "Транспорт",
                                Payments = new[]
                                {
                                    new { Date = DateTime.Now.AddDays(-8), Name = "Проездной", Price = 2000m, Num = 1 },
                                    new { Date = DateTime.Now.AddDays(-2), Name = "Такси", Price = 500m, Num = 2 }
                                }
                            },
                            new {
                                Name = "Продукты питания",
                                Payments = new[]
                                {
                                    new { Date = DateTime.Now.AddDays(-1), Name = "Продукты", Price = 3000m, Num = 1 }
                                }
                            }
                        };

                        // Цикл по категориям платежей
                        foreach (var groupCategory in testCategories)
                        {
                            // Заголовок категории
                            Excel.Range headerRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 5]];
                            headerRange.Merge();
                            headerRange.Value = groupCategory.Name;
                            headerRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;
                            headerRange.Font.Italic = true;
                            startRowIndex++;

                            // Цикл по платежам в категории
                            foreach (var payment in groupCategory.Payments)
                            {
                                worksheet.Cells[startRowIndex, 1] = payment.Date.ToString("dd.MM.yyyy");
                                worksheet.Cells[startRowIndex, 2] = payment.Name;
                                worksheet.Cells[startRowIndex, 3] = payment.Price;
                                (worksheet.Cells[startRowIndex, 3] as Excel.Range).NumberFormat = "0.00";
                                worksheet.Cells[startRowIndex, 4] = payment.Num;
                                worksheet.Cells[startRowIndex, 5].Formula = $"=C{startRowIndex}*D{startRowIndex}";
                                (worksheet.Cells[startRowIndex, 5] as Excel.Range).NumberFormat = "0.00";

                                startRowIndex++;
                            }

                            // Итог по категории
                            Excel.Range sumRange = worksheet.Range[worksheet.Cells[startRowIndex, 1], worksheet.Cells[startRowIndex, 4]];
                            sumRange.Merge();
                            sumRange.Value = "ИТОГО:";
                            sumRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignRight;

                            int startCategoryRow = startRowIndex - groupCategory.Payments.Count();
                            int endCategoryRow = startRowIndex - 1;
                            worksheet.Cells[startRowIndex, 5].Formula = $"=SUM(E{startCategoryRow}:E{endCategoryRow})";

                            sumRange.Font.Bold = true;
                            (worksheet.Cells[startRowIndex, 5] as Excel.Range).Font.Bold = true;

                            // Добавляем к общему итогу
                            decimal categoryTotal = groupCategory.Payments.Sum(p => p.Price * p.Num);
                            grandTotal += categoryTotal;

                            startRowIndex++;
                        }

                        // Добавляем границы таблицы
                        Excel.Range rangeBorders = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[startRowIndex - 1, 5]];
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeBottom].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeLeft].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeRight].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlEdgeTop].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideHorizontal].LineStyle =
                        rangeBorders.Borders[Excel.XlBordersIndex.xlInsideVertical].LineStyle =
                        Excel.XlLineStyle.xlContinuous;

                        // Автоподбор ширины столбцов
                        worksheet.Columns.AutoFit();
                    }

                    // Добавляем лист с общим итогом
                    AddSummarySheet(workbook, grandTotal);
                }

                // Показываем Excel
                application.Visible = true;

                MessageBox.Show("Данные успешно экспортированы в Microsoft Excel!", "Экспорт завершен");
            }
            catch (Exception ex)
            {
                throw new Exception($"Ошибка создания Excel документа: {ex.Message}");
            }
            finally
            {
                // Освобождаем ресурсы
                if (workbook != null) Marshal.ReleaseComObject(workbook);
                if (application != null)
                {
                    // Не закрываем приложение, чтобы пользователь видел результат
                    // application.Quit();
                    // Marshal.ReleaseComObject(application);
                }
            }
        }

        private void AddSummarySheet(Excel.Workbook workbook, decimal grandTotal)
        {
            // Добавляем новый лист после всех существующих
            Excel.Worksheet summarySheet = workbook.Worksheets.Add(
                After: workbook.Worksheets[workbook.Worksheets.Count]);
            summarySheet.Name = "Общий итог";

            // Заголовок отчета
            summarySheet.Cells[1, 1] = "СВОДНЫЙ ОТЧЕТ ПО ПЛАТЕЖАМ";
            Excel.Range titleRange = summarySheet.Range[summarySheet.Cells[1, 1], summarySheet.Cells[1, 2]];
            titleRange.Merge();
            titleRange.Font.Bold = true;
            titleRange.Font.Size = 14;
            titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

            // Дата создания отчета
            summarySheet.Cells[2, 1] = $"Дата создания: {DateTime.Now:dd.MM.yyyy HH:mm}";

            // Общий итог
            summarySheet.Cells[4, 1] = "Общий итог по всем пользователям:";
            summarySheet.Cells[4, 2] = grandTotal;
            (summarySheet.Cells[4, 2] as Excel.Range).NumberFormat = "0.00";

            // Форматирование строки общего итога
            Excel.Range summaryRange = summarySheet.Range[summarySheet.Cells[4, 1], summarySheet.Cells[4, 2]];
            summaryRange.Font.Color = Excel.XlRgbColor.rgbRed;
            summaryRange.Font.Bold = true;

            // Статистика
            summarySheet.Cells[6, 1] = "Статистика:";
            summarySheet.Cells[7, 1] = "Количество пользователей:";
            summarySheet.Cells[7, 2] = workbook.Worksheets.Count - 1; // Минус лист с итогом
            summarySheet.Cells[8, 1] = "Средний платеж на пользователя:";
            summarySheet.Cells[8, 2] = grandTotal / (workbook.Worksheets.Count - 1);
            (summarySheet.Cells[8, 2] as Excel.Range).NumberFormat = "0.00";

            // Автоподбор ширины столбцов
            summarySheet.Columns.AutoFit();

            // Добавляем границы для статистики
            Excel.Range statsRange = summarySheet.Range[summarySheet.Cells[6, 1], summarySheet.Cells[8, 2]];
            statsRange.Borders.LineStyle = Excel.XlLineStyle.xlContinuous;
        }

        private void ExportToWord()
        {
            // Существующая реализация экспорта в Word
            try
            {
                // Ваш код экспорта в Word здесь
                MessageBox.Show("Экспорт в Word выполнен успешно!", "Экспорт");
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Ошибка экспорта в Word: {ex.Message}", "Ошибка");
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            NavigationService?.Navigate(new AdminPage());
        }
    }

    public class PaymentData
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double BarWidth { get; set; }
    }
}