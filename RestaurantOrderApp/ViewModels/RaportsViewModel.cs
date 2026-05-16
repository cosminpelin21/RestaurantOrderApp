using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
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
        private readonly OrderBLL _orderBll = new OrderBLL();
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
            try
            {
                var criticalList = await _orderBll.GetCriticalStockReportAsync(10);
                CriticalStock = new ObservableCollection<Product>(criticalList);

                var statsList = await _orderBll.GetTopSellingProductsStatsAsync();
                TopProducts = new ObservableCollection<ProductStats>(statsList);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error generating reports data: " + ex.Message,
                    "Reports Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);

            }
        }
    }
}
