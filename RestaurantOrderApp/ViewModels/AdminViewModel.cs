using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _products;
        private Product _selectedProduct;
        private ObservableCollection<Category> _categories;

        public ObservableCollection<Product> Products
        {
            get => _products;
            set { _products = value; OnPropertyChanged(); }
        }

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); }
        }

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        public RelayCommand AddCommand { get; }
        public RelayCommand DeleteCommand { get; }

        public AdminViewModel()
        {
            LoadData();
            AddCommand = new RelayCommand(ExecuteAdd);
            DeleteCommand = new RelayCommand(ExecuteDelete, p => SelectedProduct != null);
        }

        private void LoadData()
        {
            using (var db = new RestaurantDbContext())
            {
                Products = new ObservableCollection<Product>(db.Products.Include(p => p.Category).ToList());
                Categories = new ObservableCollection<Category>(db.Categories.ToList());
            }
        }

        private void ExecuteAdd(object obj)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Database.ExecuteSqlRaw("EXEC AddProduct @Name={0}, @Price={1}, @PortionQuantity={2}, @TotalQuantity={3}, @CategoryId={4}, @ImagePath={5}",
                    "Produs Nou", 25.00, "300g", 50, Categories.First().CategoryId, null);
                LoadData();
            }
        }

        private void ExecuteDelete(object obj)
        {
            if (MessageBox.Show("Are you sure you are deleting the product?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                using (var db = new RestaurantDbContext())
                {
                    db.Database.ExecuteSqlRaw("EXEC DeleteProduct @ProductId={0}", SelectedProduct.ProductId);
                    LoadData();
                }
            }
        }
    }
}
