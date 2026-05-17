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
    public class StaffProductsViewModel : BaseViewModel
    {
        private readonly ProductBLL _productBll = new ProductBLL();
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private Product _selectedProduct;

        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); FillProductForm(); }
        }

        private string _pName;
        public string PName 
        {
            get => _pName;
            set { _pName = value; OnPropertyChanged(); }
        }
        private decimal _pPrice;
        public decimal PPrice 
        {
            get => _pPrice;
            set { _pPrice = value; OnPropertyChanged(); }
        }
        private string _pPortion;
        public string PPortion 
        { 
            get => _pPortion; 
            set { _pPortion = value; OnPropertyChanged(); }
        }
        private decimal _pStock;
        public decimal PStock 
        { 
            get => _pStock; 
            set { _pStock = value; OnPropertyChanged(); }
        }
        private Category _pCategory;
        public Category PCategory 
        { 
            get => _pCategory;
            set { _pCategory = value; OnPropertyChanged(); }
        }

        private string _pIngredients;

        public string PIngredients
        {
            get => _pIngredients;
            set { _pIngredients = value; OnPropertyChanged(); }
        }

        private string _pImagePath;

        public string PImagePath
        {
            get => _pIngredients;
            set { _pImagePath = value; OnPropertyChanged(); }
        }

        private string _newCategoryName;
        public string NewCategoryName 
        { 
            get => _newCategoryName; 
            set { _newCategoryName = value; OnPropertyChanged(); }
        }

        public RelayCommand NewProductModeCommand { get; }
        public RelayCommand SaveProductCommand { get; }
        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand AddCategoryCommand { get; }
        public RelayCommand DeleteCategoryCommand { get; }

        public StaffProductsViewModel()
        {
            NewProductModeCommand = new RelayCommand(_ =>
            {
                SelectedProduct = null;
                PName = "";
                PPrice = 0;
                PPortion = "";
                PStock = 0;
                PCategory = null;
                PIngredients = "";
                PImagePath = "";
            });
            SaveProductCommand = new RelayCommand(async _ => await ExecuteSaveProduct());
            DeleteProductCommand = new RelayCommand(async _ => await ExecuteDeleteProduct());
            AddCategoryCommand = new RelayCommand(async _ => await ExecuteAddCategory());
            DeleteCategoryCommand = new RelayCommand(async p => await ExecuteDeleteCategory(p as Category));

            _ = InitializeAsync();
        }

        private async Task InitializeAsync()
        {
            try
            {
                var products = await _productBll.GetAllProductsWithCategoryAsync();
                var categories = await _productBll.GetAllCategoriesAsync();

                Products.Clear();
                products.ForEach(Products.Add);

                Categories.Clear();
                categories.ForEach(Categories.Add);
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading management data: " + ex.Message,
                    "Error Initialization", System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Error);
            }
        }

        private void FillProductForm()
        {
            if (SelectedProduct == null)
                return;
            PName = SelectedProduct.Name;
            PPrice = SelectedProduct.Price;
            PPortion = SelectedProduct.PortionQuantity;
            PStock = SelectedProduct.TotalQuantity;
            PCategory = Categories.FirstOrDefault(c => c.CategoryId == SelectedProduct.CategoryId);
            PIngredients = SelectedProduct.Ingredients;
            PImagePath = SelectedProduct.ImagePath;
        }

        private async Task ExecuteSaveProduct()
        {
            if (string.IsNullOrWhiteSpace(PName) || PCategory == null)
            {
                MessageBox.Show("Please enter a name and select a category!");
                return;
            }

            try
            {
                int? productId = SelectedProduct?.ProductId;

                await _productBll.SaveProductAsync(
                    productId,
                    PName,
                    PPrice,
                    PPortion,
                    PStock,
                    PCategory.CategoryId,
                    PIngredients,
                    PImagePath
                );

                if (SelectedProduct == null)
                    MessageBox.Show("Product successfully added!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);
                else
                    MessageBox.Show("Updated product!", "Succes", MessageBoxButton.OK, MessageBoxImage.Information);

                await InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error saving product: {ex.Message}", "Warning", MessageBoxButton.OK, MessageBoxImage.Warning);
            }
        }

        private async Task ExecuteDeleteProduct()
        {
            if (SelectedProduct == null)
                return;
            try
            {
                await _productBll.DeleteProductAsync(SelectedProduct.ProductId);

                System.Windows.MessageBox.Show("The product has been successfully deleted!", "Succes",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                await InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Could not delete product: {ex.Message}", "Warning",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private async Task ExecuteAddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;

            try
            {
                await _productBll.AddCategoryAsync(NewCategoryName);

                System.Windows.MessageBox.Show("The category has been successfully added!", "Success",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Information);

                NewCategoryName = "";
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show($"Error adding category: {ex.Message}", "Warning",
                    System.Windows.MessageBoxButton.OK, System.Windows.MessageBoxImage.Warning);
            }
        }

        private async Task ExecuteDeleteCategory(Category cat)
        {
            if (cat == null) return;

            var confirm = MessageBox.Show($"Are you sure you delete category '{cat.Name}'? Attention: products in this category will remain without reference or will be deleted.", "Confirmation", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                await _productBll.DeleteCategoryAsync(cat.CategoryId);

                MessageBox.Show("The category has been successfully deleted!", "Success",
                    MessageBoxButton.OK, MessageBoxImage.Information);

                await InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on deletion: " + ex.Message);
            }
        }
    }
}