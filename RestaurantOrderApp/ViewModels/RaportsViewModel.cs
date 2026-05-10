using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantOrderApp.ViewModels
{
    public class ReportsViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _criticalStock;
        public ObservableCollection<Product> CriticalStock
        {
            get => _criticalStock;
            set { _criticalStock = value; OnPropertyChanged(); }
        }

        public class ProductStats
        {
            public string Name { get; set; }
            public int TotalSold { get; set; }
            public decimal TotalRevenue { get; set; }
        }

        private ObservableCollection<ProductStats> _topProducts;
        public ObservableCollection<ProductStats> TopProducts
        {
            get => _topProducts;
            set { _topProducts = value; OnPropertyChanged(); }
        }

        public ReportsViewModel()
        {
            _ = LoadReportsAsync();
        }

        public async Task LoadReportsAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                var critical = await db.Products
                    .FromSqlRaw("EXEC GetCriticalStock @Threshold = 10")
                    .ToListAsync();
                CriticalStock = new ObservableCollection<Product>(critical);

                var conn = db.Database.GetDbConnection();
                await conn.OpenAsync();
                using (var command = conn.CreateCommand())
                {
                    command.CommandText = "EXEC GetTopSellingProducts";
                    using (var reader = await command.ExecuteReaderAsync())
                    {
                        var stats = new ObservableCollection<ProductStats>();
                        while (await reader.ReadAsync())
                        {
                            stats.Add(new ProductStats
                            {
                                Name = reader.GetString(0),
                                TotalSold = reader.GetInt32(1),
                                TotalRevenue = reader.GetDecimal(2)
                            });
                        }
                        TopProducts = stats;
                    }
                }
            }
        }
    }
}
