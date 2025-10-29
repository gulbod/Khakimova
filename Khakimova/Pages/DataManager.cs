// DataManager.cs
using System.Collections.ObjectModel;
using System.Linq;

namespace Khakimova.Pages
{
    public static class DataManager
    {
        public static ObservableCollection<Category> Categories { get; set; }

        static DataManager()
        {
            // Инициализация только один раз при первом обращении
            if (Categories == null)
            {
                Categories = new ObservableCollection<Category>
                {
                    new Category { ID = 1, Name = "Коммунальные платежи" },
                    new Category { ID = 2, Name = "Интернет и связь" },
                    new Category { ID = 3, Name = "Транспорт" },
                    new Category { ID = 4, Name = "Продукты питания" }
                };
            }
        }

        public static int GetNextId()
        {
            return Categories.Any() ? Categories.Max(c => c.ID) + 1 : 1;
        }
    }
}