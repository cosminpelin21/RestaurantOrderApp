using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
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
        private readonly ProductBLL _productBll = new ProductBLL();
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
            try
            {
                var critical = await _productBll.GetCriticalStockProductsAsync(Threshold);

                CriticalProducts.Clear();
                critical.ForEach(CriticalProducts.Add);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading critical stock: " + ex.Message,
                    "Error", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }
    }
}