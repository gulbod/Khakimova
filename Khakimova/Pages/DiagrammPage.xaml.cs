using System;
using System.Collections.Generic;
using System.Data.Entity;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Shapes;
using Excel = Microsoft.Office.Interop.Excel;

namespace Khakimova.Pages
{
    public partial class DiagramPage : Page
    {
        private readonly List<System.Windows.Media.Color> _categoryColors = new List<System.Windows.Media.Color>
        {
            System.Windows.Media.Color.FromRgb(52, 152, 219),   // Синий
            System.Windows.Media.Color.FromRgb(46, 204, 113),   // Зеленый
            System.Windows.Media.Color.FromRgb(155, 89, 182),   // Фиолетовый
            System.Windows.Media.Color.FromRgb(241, 196, 15),   // Желтый
            System.Windows.Media.Color.FromRgb(230, 126, 34),   // Оранжевый
            System.Windows.Media.Color.FromRgb(231, 76, 60),    // Красный
            System.Windows.Media.Color.FromRgb(26, 188, 156),   // Бирюзовый
            System.Windows.Media.Color.FromRgb(149, 165, 166)   // Серый
        };

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

                    // Загружаем только два типа визуализации
                    var visualizationTypes = new List<string>
                    {
                        "Столбчатая диаграмма",
                        "Круговая диаграмма"
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
                MessageBox.Show("Ошибка загрузки данных: " + ex.Message);
            }
        }

        private void UpdateChart(object sender, SelectionChangedEventArgs e)
        {
            if (CmbUser.SelectedItem is User currentUser && CmbDiagram.SelectedItem is string diagramType)
            {
                try
                {
                    // Очищаем предыдущие данные
                    ClearAllCharts();

                    // Получаем реальные данные из базы для выбранного пользователя
                    var paymentData = GetRealPaymentData(currentUser.ID);

                    if (!paymentData.Any())
                    {
                        MessageBox.Show("Нет данных для выбранного пользователя");
                        return;
                    }

                    // Рассчитываем ширину баров и назначаем цвета
                    decimal maxAmount = paymentData.Max(p => p.Amount);
                    for (int i = 0; i < paymentData.Count; i++)
                    {
                        var payment = paymentData[i];
                        payment.BarWidth = (double)(payment.Amount / maxAmount) * 300;
                        payment.Color = _categoryColors[i % _categoryColors.Count];
                        payment.DarkColor = DarkenColor(payment.Color, 0.3);
                        payment.LightColor = LightenColor(payment.Color, 0.3);
                    }

                    // Отображаем выбранный тип диаграммы
                    switch (diagramType)
                    {
                        case "Столбчатая диаграмма":
                            ShowBarChart(paymentData);
                            break;
                        case "Круговая диаграмма":
                            ShowPieChart(paymentData);
                            break;
                    }

                    // Обновляем общую сумму
                    decimal total = paymentData.Sum(p => p.Amount);
                    TotalText.Text = "ИТОГО: " + total.ToString("N2") + " ₽";

                    // Обновляем заголовок пользователя
                    if (currentUser != null)
                    {
                        MainChartTitle.Text = "Распределение платежей по категориям: " + currentUser.FIO;
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Ошибка обновления данных: " + ex.Message);
                }
            }
        }

        private List<PaymentData> GetRealPaymentData(int userId)
        {
            var result = new List<PaymentData>();

            try
            {
                using (var db = new Khakimova_DB_PaymentEntities())
                {
                    // Сначала получаем все платежи для пользователя
                    var payments = db.Payment
                        .Where(p => p.UserID == userId)
                        .Include("Category") // Включаем связанные категории
                        .ToList();

                    // Группируем и суммируем уже в памяти
                    var groupedPayments = payments
                        .GroupBy(p => p.Category?.Name ?? "Без категории")
                        .Select(g => new PaymentData
                        {
                            Category = g.Key,
                            Amount = g.Sum(p => GetPaymentAmount(p))
                        })
                        .Where(p => p.Amount > 0)
                        .ToList();

                    result = groupedPayments;
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка загрузки данных из БД: " + ex.Message);
                // Возвращаем тестовые данные в случае ошибки
                return GenerateTestPaymentData();
            }

            return result;
        }

        // Вспомогательный метод для получения суммы платежа
        private decimal GetPaymentAmount(Khakimova.Payment payment)
        {
            try
            {
                // Используем рефлексию для поиска свойства с суммой
                var properties = payment.GetType().GetProperties();

                // Список возможных названий свойств для суммы
                var amountPropertyNames = new List<string>
                {
                    "Amount", "Summa", "PaymentAmount", "Total", "Value",
                    "Sum", "PaymentSum", "PaymentValue", "Price", "Cost"
                };

                foreach (var propName in amountPropertyNames)
                {
                    var prop = properties.FirstOrDefault(p =>
                        p.Name.Equals(propName, StringComparison.OrdinalIgnoreCase) &&
                        (p.PropertyType == typeof(decimal) || p.PropertyType == typeof(decimal?)));

                    if (prop != null)
                    {
                        var value = prop.GetValue(payment);
                        if (value != null)
                        {
                            if (value is decimal decimalValue)
                            {
                                return decimalValue;
                            }
                            if (value is decimal?)
                            {
                                var nullableDecimal = (decimal?)value;
                                if (nullableDecimal.HasValue)
                                {
                                    return nullableDecimal.Value;
                                }
                            }
                        }
                    }
                }

                // Если не нашли подходящее свойство, пробуем получить первое decimal свойство
                foreach (var prop in properties)
                {
                    if (prop.PropertyType == typeof(decimal) || prop.PropertyType == typeof(decimal?))
                    {
                        var value = prop.GetValue(payment);
                        if (value != null)
                        {
                            if (value is decimal decimalValue)
                            {
                                return decimalValue;
                            }
                            if (value is decimal?)
                            {
                                var nullableDecimal = (decimal?)value;
                                if (nullableDecimal.HasValue)
                                {
                                    return nullableDecimal.Value;
                                }
                            }
                        }
                    }
                }

                return 0m;
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка при получении суммы платежа: " + ex.Message);
                return 0m;
            }
        }

        private List<PaymentData> GenerateTestPaymentData()
        {
            // Тестовые данные на случай ошибки загрузки из БД
            return new List<PaymentData>
            {
                new PaymentData { Category = "Интернет и связь", Amount = 1200 },
                new PaymentData { Category = "Транспорт", Amount = 3500 },
                new PaymentData { Category = "Продукты питания", Amount = 8000 },
                new PaymentData { Category = "Развлечения", Amount = 2500 }
            };
        }

        private void ClearAllCharts()
        {
            BarChartVisualization.ItemsSource = null;
            PieChartCanvas.Children.Clear();
            PieChartLegend.ItemsSource = null;

            BarChartScroll.Visibility = Visibility.Collapsed;
            PieChartScroll.Visibility = Visibility.Collapsed;
        }

        private void ShowBarChart(List<PaymentData> data)
        {
            BarChartVisualization.ItemsSource = data;
            BarChartScroll.Visibility = Visibility.Visible;
        }

        private void ShowPieChart(List<PaymentData> data)
        {
            // Очищаем canvas
            PieChartCanvas.Children.Clear();

            // Устанавливаем данные для легенды
            PieChartLegend.ItemsSource = data;

            // Параметры круговой диаграммы
            double centerX = 150;
            double centerY = 150;
            double radius = 120;
            double totalAmount = (double)data.Sum(p => p.Amount);

            // Рисуем сегменты круговой диаграммы
            double startAngle = 0;

            for (int i = 0; i < data.Count; i++)
            {
                var payment = data[i];
                double percentage = totalAmount > 0 ? (double)payment.Amount / totalAmount : 0;
                double sweepAngle = percentage * 360;

                // Создаем сегмент
                PathFigure pathFigure = new PathFigure();
                pathFigure.StartPoint = new Point(centerX, centerY);

                // Линия к начальной точке дуги
                Point startPoint = GetPointOnCircle(centerX, centerY, radius, startAngle);
                pathFigure.Segments.Add(new LineSegment(startPoint, true));

                // Дуга
                Point endPoint = GetPointOnCircle(centerX, centerY, radius, startAngle + sweepAngle);
                pathFigure.Segments.Add(new ArcSegment(
                    endPoint,
                    new Size(radius, radius),
                    0,
                    sweepAngle > 180,
                    SweepDirection.Clockwise,
                    true));

                // Замыкаем фигуру
                pathFigure.Segments.Add(new LineSegment(new Point(centerX, centerY), true));

                PathGeometry pathGeometry = new PathGeometry();
                pathGeometry.Figures.Add(pathFigure);

                System.Windows.Shapes.Path path = new System.Windows.Shapes.Path
                {
                    Fill = new SolidColorBrush(payment.Color),
                    Stroke = Brushes.White,
                    StrokeThickness = 2,
                    Data = pathGeometry,
                    ToolTip = payment.Category + ": " + payment.Amount.ToString("C") + " (" + percentage.ToString("P1") + ")"
                };

                PieChartCanvas.Children.Add(path);

                // Добавляем подпись для сегмента (если сегмент достаточно большой)
                if (percentage > 0.05) // Если сегмент больше 5%
                {
                    double labelAngle = startAngle + sweepAngle / 2;
                    Point labelPoint = GetPointOnCircle(centerX, centerY, radius * 0.7, labelAngle);

                    TextBlock label = new TextBlock
                    {
                        Text = percentage.ToString("P0"),
                        Foreground = Brushes.White,
                        FontWeight = FontWeights.Bold,
                        FontSize = 10,
                        HorizontalAlignment = HorizontalAlignment.Center,
                        VerticalAlignment = VerticalAlignment.Center
                    };

                    Canvas.SetLeft(label, labelPoint.X - 15);
                    Canvas.SetTop(label, labelPoint.Y - 8);
                    PieChartCanvas.Children.Add(label);
                }

                startAngle += sweepAngle;
            }

            // Показываем круговую диаграмму
            PieChartScroll.Visibility = Visibility.Visible;
        }

        private Point GetPointOnCircle(double centerX, double centerY, double radius, double angle)
        {
            double angleRad = (angle - 90) * Math.PI / 180;
            double x = centerX + radius * Math.Cos(angleRad);
            double y = centerY + radius * Math.Sin(angleRad);
            return new Point(x, y);
        }

        private System.Windows.Media.Color DarkenColor(System.Windows.Media.Color color, double factor)
        {
            return System.Windows.Media.Color.FromRgb(
                (byte)(color.R * (1 - factor)),
                (byte)(color.G * (1 - factor)),
                (byte)(color.B * (1 - factor))
            );
        }

        private System.Windows.Media.Color LightenColor(System.Windows.Media.Color color, double factor)
        {
            return System.Windows.Media.Color.FromRgb(
                (byte)(color.R + (255 - color.R) * factor),
                (byte)(color.G + (255 - color.G) * factor),
                (byte)(color.B + (255 - color.B) * factor)
            );
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
                MessageBox.Show("Ошибка экспорта в Excel: " + ex.Message, "Ошибка");
            }
        }

        private void BtnExportWord_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ExportToWord();
                MessageBox.Show("Данные успешно экспортированы!", "Экспорт");
            }
            catch (Exception ex)
            {
                MessageBox.Show("Ошибка экспорта: " + ex.Message, "Ошибка");
            }
        }

        private void ExportToExcel()
        {
            Excel.Application excelApp = null;
            Excel.Workbook workbook = null;
            Excel.Worksheet worksheet = null;

            try
            {
                excelApp = new Excel.Application();
                excelApp.Visible = false;
                excelApp.DisplayAlerts = false;

                workbook = excelApp.Workbooks.Add();
                worksheet = workbook.Worksheets[1];
                worksheet.Name = "Отчет по платежам";

                // Заголовок отчета
                worksheet.Cells[1, 1] = "РАСПРЕДЕЛЕНИЕ ПЛАТЕЖЕЙ ПО КАТЕГОРИЯМ";
                Excel.Range titleRange = worksheet.Range[worksheet.Cells[1, 1], worksheet.Cells[1, 4]];
                titleRange.Merge();
                titleRange.Font.Bold = true;
                titleRange.Font.Size = 16;
                titleRange.HorizontalAlignment = Excel.XlHAlign.xlHAlignCenter;

                // Информация о пользователе
                int row = 3;
                var selectedUser = CmbUser.SelectedItem as User;
                if (selectedUser != null)
                {
                    worksheet.Cells[row, 1] = "Пользователь:";
                    worksheet.Cells[row, 2] = selectedUser.FIO;
                    row++;
                }

                worksheet.Cells[row, 1] = "Дата создания:";
                worksheet.Cells[row, 2] = DateTime.Now.ToString("dd.MM.yyyy HH:mm");
                row += 2;

                // Таблица данных
                worksheet.Cells[row, 1] = "Категория";
                worksheet.Cells[row, 2] = "Сумма";
                worksheet.Cells[row, 3] = "Процент";

                Excel.Range headerRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 3]];
                headerRange.Font.Bold = true;
                headerRange.Interior.Color = 0xF0F0F0; // Светло-серый цвет
                row++;

                // Получаем реальные данные для экспорта
                var paymentData = selectedUser != null ?
                    GetRealPaymentData(selectedUser.ID) : GenerateTestPaymentData();
                decimal totalAmount = paymentData.Sum(p => p.Amount);

                foreach (var payment in paymentData)
                {
                    double percentage = totalAmount > 0 ? (double)(payment.Amount / totalAmount) * 100 : 0;

                    worksheet.Cells[row, 1] = payment.Category;
                    worksheet.Cells[row, 2] = (double)payment.Amount;
                    worksheet.Cells[row, 3] = percentage / 100.0;

                    (worksheet.Cells[row, 2] as Excel.Range).NumberFormat = "#,##0.00";
                    (worksheet.Cells[row, 3] as Excel.Range).NumberFormat = "0.00%";

                    row++;
                }

                // Итоговая строка
                worksheet.Cells[row, 1] = "ИТОГО:";
                worksheet.Cells[row, 2] = (double)totalAmount;
                worksheet.Cells[row, 3] = 1.0;

                Excel.Range totalRange = worksheet.Range[worksheet.Cells[row, 1], worksheet.Cells[row, 3]];
                totalRange.Font.Bold = true;
                totalRange.Interior.Color = 0x90EE90; // Светло-зеленый цвет
                (worksheet.Cells[row, 2] as Excel.Range).NumberFormat = "#,##0.00";
                (worksheet.Cells[row, 3] as Excel.Range).NumberFormat = "0.00%";

                // Добавляем диаграмму
                row += 2;
                AddChartToExcel(worksheet, paymentData, row);

                // Автоматическая подгонка ширины столбцов
                worksheet.Columns.AutoFit();

                // Сохраняем файл
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Excel Files (*.xlsx)|*.xlsx",
                    FileName = "Отчет_по_платежам_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
                };

                if (saveDialog.ShowDialog() == true)
                {
                    workbook.SaveAs(saveDialog.FileName);
                    excelApp.Visible = true;
                    MessageBox.Show("Данные успешно экспортированы в Microsoft Excel!", "Экспорт завершен");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("документ " + ex.Message);
            }
            finally
            {
                // Правильное освобождение ресурсов
                if (worksheet != null) Marshal.ReleaseComObject(worksheet);
                if (workbook != null)
                {
                    workbook.Close(false);
                    Marshal.ReleaseComObject(workbook);
                }
                if (excelApp != null)
                {
                    excelApp.Quit();
                    Marshal.ReleaseComObject(excelApp);
                }

                GC.Collect();
                GC.WaitForPendingFinalizers();
                GC.Collect();
            }
        }

        private void AddChartToExcel(Excel.Worksheet worksheet, List<PaymentData> data, int startRow)
        {
            try
            {
                // Создаем диаграмму
                Excel.ChartObjects chartObjects = (Excel.ChartObjects)worksheet.ChartObjects();
                Excel.ChartObject chartObject = chartObjects.Add(100, startRow * 15, 400, 250);
                Excel.Chart chart = chartObject.Chart;

                // Подготавливаем данные для диаграммы
                int dataCount = data.Count;
                int dataStartRow = startRow - dataCount - 3;

                // Диапазон данных для диаграммы
                Excel.Range categoryRange = worksheet.Range[worksheet.Cells[dataStartRow, 1], worksheet.Cells[dataStartRow + dataCount - 1, 1]];
                Excel.Range valueRange = worksheet.Range[worksheet.Cells[dataStartRow, 2], worksheet.Cells[dataStartRow + dataCount - 1, 2]];

                // Устанавливаем данные для диаграммы
                chart.ChartType = Excel.XlChartType.xlPie;
                chart.SetSourceData(valueRange);

                // Настраиваем заголовок
                chart.HasTitle = true;
                chart.ChartTitle.Text = "Распределение платежей по категориям";

                // Настраиваем подписи данных
                chart.ApplyDataLabels();
                Excel.DataLabels dataLabels = chart.SeriesCollection(1).DataLabels;
                dataLabels.ShowPercentage = true;
                dataLabels.ShowValue = false;
                dataLabels.ShowCategoryName = true;

                // Освобождаем COM-объекты
                Marshal.ReleaseComObject(dataLabels);
                Marshal.ReleaseComObject(categoryRange);
                Marshal.ReleaseComObject(valueRange);
                Marshal.ReleaseComObject(chart);
                Marshal.ReleaseComObject(chartObject);
                Marshal.ReleaseComObject(chartObjects);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Ошибка при создании диаграммы: " + ex.Message);
            }
        }

        private void ExportToWord()
        {
            try
            {
                // Получаем реальные данные для экспорта
                var selectedUser = CmbUser.SelectedItem as User;
                var paymentData = selectedUser != null ?
                    GetRealPaymentData(selectedUser.ID) : GenerateTestPaymentData();
                decimal totalAmount = paymentData.Sum(p => p.Amount);

                // Создаем HTML контент для Word
                string htmlContent = "<html>" +
                    "<head>" +
                    "<meta charset='utf-8'>" +
                    "<title>Отчет по платежам</title>" +
                    "<style>" +
                    "body { font-family: Arial, sans-serif; margin: 20px; }" +
                    "h1 { color: #333; text-align: center; }" +
                    "table { width: 100%; border-collapse: collapse; margin-top: 20px; }" +
                    "th, td { border: 1px solid #ddd; padding: 8px; text-align: left; }" +
                    "th { background-color: #f2f2f2; font-weight: bold; }" +
                    ".total { background-color: #e8f5e8; font-weight: bold; }" +
                    ".percentage { text-align: right; }" +
                    "</style>" +
                    "</head>" +
                    "<body>" +
                    "<h1>РАСПРЕДЕЛЕНИЕ ПЛАТЕЖЕЙ ПО КАТЕГОРИЯМ</h1>" +
                    "<p><strong>Пользователь:</strong> " + (selectedUser?.FIO ?? "Не выбран") + "</p>" +
                    "<p><strong>Дата создания:</strong> " + DateTime.Now.ToString("dd.MM.yyyy HH:mm") + "</p>" +
                    "<table>" +
                    "<tr>" +
                    "<th>Категория</th>" +
                    "<th>Сумма</th>" +
                    "<th>Процент</th>" +
                    "</tr>";

                foreach (var payment in paymentData)
                {
                    double percentage = totalAmount > 0 ? (double)(payment.Amount / totalAmount) * 100 : 0;
                    htmlContent += "<tr>" +
                        "<td>" + payment.Category + "</td>" +
                        "<td>" + payment.Amount.ToString("N2") + " ₽</td>" +
                        "<td class='percentage'>" + percentage.ToString("F1") + "%</td>" +
                        "</tr>";
                }

                htmlContent += "<tr class='total'>" +
                    "<td><strong>ИТОГО:</strong></td>" +
                    "<td><strong>" + totalAmount.ToString("N2") + " ₽</strong></td>" +
                    "<td class='percentage'><strong>100%</strong></td>" +
                    "</tr>" +
                    "</table>" +
                    "<div style='margin-top: 30px;'>" +
                    "<h3>Диаграмма распределения:</h3>" +
                    "<ul>";

                foreach (var payment in paymentData)
                {
                    double percentage = totalAmount > 0 ? (double)(payment.Amount / totalAmount) * 100 : 0;
                    htmlContent += "<li>" + payment.Category + ": " + percentage.ToString("F1") + "% (" + payment.Amount.ToString("N2") + " ₽)</li>";
                }

                htmlContent += "</ul>" +
                    "</div>" +
                    "</body>" +
                    "</html>";

                // Сохраняем HTML файл
                var saveDialog = new Microsoft.Win32.SaveFileDialog
                {
                    Filter = "Word Documents (*.doc)|*.doc|HTML Files (*.html)|*.html",
                    FileName = "Отчет_по_платежам_" + DateTime.Now.ToString("yyyyMMdd_HHmmss")
                };

                if (saveDialog.ShowDialog() == true)
                {
                    File.WriteAllText(saveDialog.FileName, htmlContent);

                    // Открываем файл в Word
                    Process.Start(new ProcessStartInfo
                    {
                        FileName = saveDialog.FileName,
                        UseShellExecute = true
                    });

                    MessageBox.Show("Данные успешно экспортированы в документ!", "Экспорт завершен");
                }
            }
            catch (Exception ex)
            {
                throw new Exception("документ" + ex.Message);
            }
        }

        private void BtnBack_Click(object sender, RoutedEventArgs e)
        {
            if (NavigationService != null)
            {
                NavigationService.Navigate(new AdminPage());
            }
        }
    }

    public class PaymentData
    {
        public string Category { get; set; }
        public decimal Amount { get; set; }
        public double BarWidth { get; set; }
        public System.Windows.Media.Color Color { get; set; }
        public System.Windows.Media.Color DarkColor { get; set; }
        public System.Windows.Media.Color LightColor { get; set; }
    }
}