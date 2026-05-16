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
    public class StaffProductsViewModel : BaseViewModel
    {
        public ObservableCollection<Product> Products { get; set; } = new ObservableCollection<Product>();
        public ObservableCollection<Category> Categories { get; set; } = new ObservableCollection<Category>();

        private Product _selectedProduct;
        public Product SelectedProduct
        {
            get => _selectedProduct;
            set { _selectedProduct = value; OnPropertyChanged(); FillProductForm(); }
        }

        private string _pName; public string PName { get => _pName; set { _pName = value; OnPropertyChanged(); } }
        private decimal _pPrice; public decimal PPrice { get => _pPrice; set { _pPrice = value; OnPropertyChanged(); } }
        private string _pPortion; public string PPortion { get => _pPortion; set { _pPortion = value; OnPropertyChanged(); } }
        private decimal _pStock; public decimal PStock { get => _pStock; set { _pStock = value; OnPropertyChanged(); } }
        private Category _pCategory; public Category PCategory { get => _pCategory; set { _pCategory = value; OnPropertyChanged(); } }

        private string _pIngredients;
        public string PIngredients
        {
            get => _pIngredients;
            set { _pIngredients = value; OnPropertyChanged(); }
        }
        private string _pImagePath;
        public string PImagePath
        {
            get => _pImagePath;
            set { _pImagePath = value; OnPropertyChanged(); }
        }
        private string _newCategoryName;
        public string NewCategoryName { get => _newCategoryName; set { _newCategoryName = value; OnPropertyChanged(); } }

        public RelayCommand NewProductModeCommand { get; }
        public RelayCommand SaveProductCommand { get; }
        public RelayCommand DeleteProductCommand { get; }
        public RelayCommand AddCategoryCommand { get; }
        public RelayCommand DeleteCategoryCommand { get; }

        public StaffProductsViewModel()
        {
            NewProductModeCommand = new RelayCommand(_ => {
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
            using var db = new RestaurantDbContext();
            var products = await db.Products.Include(p => p.Category).ToListAsync();
            var categories = await db.Categories.ToListAsync();

            Products.Clear();
            products.ForEach(Products.Add);
            Categories.Clear();
            categories.ForEach(Categories.Add);
        }

        private void FillProductForm()
        {
            if (SelectedProduct == null) return;
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

            using var db = new RestaurantDbContext();

            if (SelectedProduct == null)
            {
                var newProd = new Product
                {
                    Name = PName,
                    Price = PPrice,
                    PortionQuantity = PPortion,
                    TotalQuantity = PStock,
                    CategoryId = PCategory.CategoryId,
                    Ingredients = PIngredients,
                    ImagePath = PImagePath
                };
                db.Products.Add(newProd);
                MessageBox.Show("Product successfully added!");
            }
            else 
            {
                var p = await db.Products.FindAsync(SelectedProduct.ProductId);
                if (p != null)
                {
                    p.Name = PName;
                    p.Price = PPrice;
                    p.PortionQuantity = PPortion;
                    p.TotalQuantity = PStock;
                    p.CategoryId = PCategory.CategoryId;
                    p.Ingredients = PIngredients;
                    p.ImagePath = PImagePath;
                    MessageBox.Show("Updated product!");
                }
            }

            await db.SaveChangesAsync();
            await InitializeAsync();
        }

        private async Task ExecuteDeleteProduct()
        {
            if (SelectedProduct == null) return;
            using var db = new RestaurantDbContext();
            var p = await db.Products.FindAsync(SelectedProduct.ProductId);
            db.Products.Remove(p);
            await db.SaveChangesAsync();
            await InitializeAsync();
        }

        private async Task ExecuteAddCategory()
        {
            if (string.IsNullOrWhiteSpace(NewCategoryName)) return;
            using var db = new RestaurantDbContext();
            db.Categories.Add(new Category { Name = NewCategoryName });
            await db.SaveChangesAsync();
            NewCategoryName = "";
            await InitializeAsync();
        }

        private async Task ExecuteDeleteCategory(Category cat)
        {
            if (cat == null) return;

            var confirm = MessageBox.Show($"Are you sure you delete category '{cat.Name}'? Attention: products in this category will remain without reference or will be deleted.", "Confirmation", MessageBoxButton.YesNo);
            if (confirm != MessageBoxResult.Yes) return;

            try
            {
                using var db = new RestaurantDbContext();

                bool hasProducts = await db.Products.AnyAsync(p => p.CategoryId == cat.CategoryId);
                if (hasProducts)
                {
                    MessageBox.Show("You cannot delete this category because it contains active products! Delete products first.");
                    return;
                }

                var dbCat = await db.Categories.FindAsync(cat.CategoryId);
                if (dbCat != null)
                {
                    db.Categories.Remove(dbCat);
                    await db.SaveChangesAsync();
                }
                await InitializeAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error on deletion: " + ex.Message);
            }
        }
    }
}
