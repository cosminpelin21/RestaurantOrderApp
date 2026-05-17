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
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class AdminViewModel : BaseViewModel
    {
        private readonly ProductBLL _productBll = new ProductBLL();
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
            try
            {
                var products = _productBll.GetProductsForAdmin();
                var categories = _productBll.GetCategoriesForAdmin();

                Products = new ObservableCollection<Product>(products);
                Categories = new ObservableCollection<Category>(categories);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading admin panel: " + ex.Message, "Error");
            }
        }

        private void ExecuteAdd(object obj)
        {
            if (Categories == null || !Categories.Any())
            {
                MessageBox.Show("There is no category in the database to associate the new product!");
                return;
            }

            try
            {
                int firstCategoryId = Categories.First().CategoryId;

                _productBll.CreateProductFromAdmin("New Product", 25.00m, "300g", 50, firstCategoryId, null);

                MessageBox.Show("The default product has been successfully added via the stored procedure!");

                LoadData();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error executing the add procedure: " + ex.Message, "Warning");
            }
        }

        private void ExecuteDelete(object obj)
        {
            if (SelectedProduct == null)
            {
                MessageBox.Show("Please select a product from the table first to delete it.");
                return;
            }

            if (MessageBox.Show("Are you sure you are deleting the product?", "Confirmation", MessageBoxButton.YesNo) == MessageBoxResult.Yes)
            {
                try
                {
                    _productBll.RemoveProductFromAdmin(SelectedProduct.ProductId);

                    MessageBox.Show("The product has been permanently removed from the menu!");
                    LoadData();
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Error executing deletion procedure: " + ex.Message, "Warning");
                }
            }
        }
    }
}