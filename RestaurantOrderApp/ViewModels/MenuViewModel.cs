using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
using RestaurantOrderApp.Models;
using RestaurantOrderApp.Views;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class MenuViewModel : BaseViewModel
    {
        private readonly ProductBLL _productBll = new ProductBLL();
        private static readonly SemaphoreSlim _searchLock = new SemaphoreSlim(1, 1);
        private bool _isLoading;

        public bool IsLoading
        {
            get => _isLoading;
            set { _isLoading = value; OnPropertyChanged(); }
        }

        private string _statusMessage;

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        private bool _showNotification;

        public bool ShowNotification
        {
            get => _showNotification;
            set { _showNotification = value; OnPropertyChanged(); }
        }

        public bool IsUserLoggedIn => RestaurantOrderApp.Helpers.UserSession.CurrentUser != null;
        private ObservableCollection<Product> _products;
        private ObservableCollection<Product> _cartItems = new ObservableCollection<Product>();
        private string _searchKeyword = "";
        public RelayCommand AddToCartCommand { get; }
        public RelayCommand OpenCartCommand { get; set; }

        public ObservableCollection<Product> Products
        {
            get => _products;
            set
            {
                _products = value;
                OnPropertyChanged();
            }
        }

        public ObservableCollection<Product> CartItems
        {
            get => _cartItems ?? (_cartItems = new ObservableCollection<Product>());
            set { _cartItems = value; OnPropertyChanged(); }
        }

        public string SearchKeyword
        {
            get => _searchKeyword;
            set
            {
                if (_searchKeyword == value) return;
                _searchKeyword = value;
                OnPropertyChanged();
                _ = ExecuteSearchAsync();
            }
        }

        private ObservableCollection<Category> _categories;

        public ObservableCollection<Category> Categories
        {
            get => _categories;
            set { _categories = value; OnPropertyChanged(); }
        }

        private Category _selectedCategory;

        public Category SelectedCategory
        {
            get => _selectedCategory;
            set
            {
                if (_selectedCategory == value) return;
                _selectedCategory = value;
                OnPropertyChanged();
                _ = ExecuteSearchAsync();
            }
        }

        private ObservableCollection<Allergen> _allergensList;

        public ObservableCollection<Allergen> AllergensList
        {
            get => _allergensList;
            set { _allergensList = value; OnPropertyChanged(); }
        }

        private Allergen _selectedSearchAllergen;

        //public Allergen _selectedSearchAllergenProperty
        //{
        //    get => _selectedSearchAllergen;
        //    set
        //    {
        //        if (_selectedSearchAllergen == value) return;
        //        _selectedSearchAllergen = value;
        //        OnPropertyChanged();
        //        _ = ExecuteSearchAsync();
        //    }
        //}

        public Allergen SelectedSearchAllergen
        {
            get => _selectedSearchAllergen;
            set
            {
                if (_selectedSearchAllergen == value) return;
                _selectedSearchAllergen = value;
                OnPropertyChanged();
                _ = ExecuteSearchAsync();
            }
        }

        private bool _excludeAllergen = true;

        public bool ExcludeAllergen
        {
            get => _excludeAllergen;
            set
            {
                if (_excludeAllergen == value) return;
                _excludeAllergen = value;
                OnPropertyChanged();
                _ = ExecuteSearchAsync();
            }
        }

        public RelayCommand ClearAllergenFilterCommand { get; }

        public MenuViewModel()
        {
            Products = new ObservableCollection<Product>();
            Categories = new ObservableCollection<Category>();
            StatusMessage = string.Empty;
            AddToCartCommand = new RelayCommand(async p =>
            {
                if (p is Product product)
                {
                    int alreadyInCart = CartItems.Count(item => item.ProductId == product.ProductId);
                    if (product.TotalQuantity - alreadyInCart <= 0)
                    {
                        StatusMessage = "OUT OF STOCK";
                        ShowNotification = true;
                        await Task.Delay(2000);
                        ShowNotification = false;
                        return;
                    }

                    CartItems.Add(product);
                    StatusMessage = $"{product.Name.ToUpper()} ADDED TO SELECTION";
                    ShowNotification = true;
                    await Task.Delay(2000);
                    ShowNotification = false;
                }
            });
            OpenCartCommand = new RelayCommand(async p =>
            {
                try
                {
                    var cartWin = new CartView(CartItems);
                    cartWin.ShowDialog();
                    await ExecuteSearchAsync();
                }
                catch (Exception ex)
                {
                    string realError = ex.InnerException != null ? ex.InnerException.Message : ex.Message;
                    MessageBox.Show("Real cause: " + realError);
                }
            });
            ClearAllergenFilterCommand = new RelayCommand(p => SelectedSearchAllergen = null);
            LoadCategoriesAsync();
        }

        public async Task InitializeAsync()
        {
            await LoadCategoriesAsync();
            await LoadAllergensAsync();
            await ExecuteSearchAsync();
        }

        private async Task LoadAllergensAsync()
        {
            try
            {
                var allergens = await _productBll.GetAllAllergensAsync();
                AllergensList = new ObservableCollection<Allergen>(allergens);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading allergens: " + ex.Message);
            }
        }

        private async Task LoadCategoriesAsync()
        {
            try
            {
                var cats = await _productBll.GetAllCategoriesOrderedAsync();
                Categories = new ObservableCollection<Category>(cats);
            }
            catch (Exception ex)
            {
                System.Diagnostics.Debug.WriteLine("Error loading categories: " + ex.Message);
            }
        }

        private async Task ExecuteSearchAsync()
        {
            await _searchLock.WaitAsync();

            try
            {
                IsLoading = true;
                var allItems = await _productBll.GetAssembledMenuAsync();

                if (SelectedCategory != null)
                {
                    allItems = allItems.Where(p => p.CategoryId == SelectedCategory.CategoryId).ToList();
                }

                if (!string.IsNullOrWhiteSpace(SearchKeyword))
                {
                    allItems = allItems.Where(p => p.Name.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)).ToList();
                }

                if (SelectedSearchAllergen != null)
                {
                    int currentAllergenId = SelectedSearchAllergen.AllergenId;
                    if (ExcludeAllergen)
                    {
                        allItems = allItems.Where(p => p.Allergens == null || !p.Allergens.Any(a => a.AllergenId == currentAllergenId)).ToList();
                    }
                    else
                    {
                        allItems = allItems.Where(p => p.Allergens != null && p.Allergens.Any(a => a.AllergenId == currentAllergenId)).ToList();
                    }
                }

                var sortedItems = allItems
                    .OrderBy(p => p.Category?.Name ?? "")
                    .ThenBy(p => p.Name)
                    .ToList();

                Products = new ObservableCollection<Product>(sortedItems);
            }
            catch (Exception ex)
            {
                StatusMessage = "BACKSTAGE ERROR: COULD NOT LOAD MENU";
                ShowNotification = true;
                await Task.Delay(3000);
                ShowNotification = false;
            }
            finally
            {
                IsLoading = false;
                _searchLock.Release();
            }
        }

        public void RefreshLoginStatus()
        {
            OnPropertyChanged(nameof(IsUserLoggedIn));
        }
    }
}