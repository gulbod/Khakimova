using System.Windows.Controls;

namespace Khakimova.Pages
{
    public class BasePage : Page
    {
        public BasePage()
        {
            this.Loaded += (s, e) => UpdateTheme();
        }

        protected virtual void UpdateTheme()
        {
            // Принудительное обновление визуального дерева
            this.InvalidateVisual();
            this.InvalidateArrange();
            this.InvalidateMeasure();
        }
    }
}