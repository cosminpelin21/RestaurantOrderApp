using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Models;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOrderApp.ViewModels
{
    public class StaffReportsViewModel : BaseViewModel
    {
        public ObservableCollection<Product> CriticalProducts { get; set; } = new ObservableCollection<Product>();

        private int _threshold;
        public int Threshold
        {
            get => _threshold;
            set { _threshold = value; OnPropertyChanged(); }
        }

        public StaffReportsViewModel()
        {
            string thresholdConfig = ConfigurationManager.AppSettings["CriticalStockThreshold"];
            Threshold = int.TryParse(thresholdConfig, out int result) ? result : 10;

            _ = LoadCriticalStockAsync();
        }

        public async Task LoadCriticalStockAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                var critical = await db.Products
                    .Include(p => p.Category)
                    .Where(p => p.TotalQuantity <= Threshold)
                    .OrderBy(p => p.TotalQuantity)
                    .ToListAsync();

                CriticalProducts.Clear();
                critical.ForEach(CriticalProducts.Add);
            }
        }
    }
}