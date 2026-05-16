using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Diagnostics;
using RestaurantOrderApp.Helpers;
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
            using (var db = new RestaurantDbContext())
            {
                var alergeni = await db.Allergens.OrderBy(a => a.Name).ToListAsync();
                AllergensList = new ObservableCollection<Allergen>(alergeni);
            }
        }
        private async Task LoadCategoriesAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                var cats = await db.Categories.OrderBy(c => c.Name).ToListAsync();
                Categories = new ObservableCollection<Category>(cats);
            }
        }

        private async Task ExecuteSearchAsync()
        {
            await _searchLock.WaitAsync();

            try
            {
                IsLoading = true;
                using (var db = new RestaurantDbContext())
                {
                    var query = db.Products
                        .Include(p => p.Category)
                        .Include(p => p.Allergens)
                        .AsQueryable();

                    if (SelectedCategory != null)
                    {
                        query = query.Where(p => p.CategoryId == SelectedCategory.CategoryId);
                    }

                    if (!string.IsNullOrWhiteSpace(SearchKeyword))
                    {
                        query = query.Where(p => p.Name.Contains(SearchKeyword));
                    }

                    if (SelectedSearchAllergen != null)
                    {
                        int currentAllergenId = SelectedSearchAllergen.AllergenId;

                        if (ExcludeAllergen)
                        {
                            query = query.Where(p => !p.Allergens.Any(a => a.AllergenId == currentAllergenId));
                        }
                        else
                        {
                            query = query.Where(p => p.Allergens.Any(a => a.AllergenId == currentAllergenId));
                        }
                    }

                    var regularProducts = await query.ToListAsync();

                    decimal discountPercent = 10;
                    var discountSetting = System.Configuration.ConfigurationManager.AppSettings["DiscountPercent"];
                    if (discountSetting != null)
                    {
                        decimal.TryParse(discountSetting, out discountPercent);
                    }

                    var complexMenus = await db.Menus
                        .Include(m => m.Category)
                        .Include(m => m.Products)
                            .ThenInclude(p => p.Allergens)
                        .ToListAsync();

                    var convertedMenus = new List<Product>();

                    foreach (var menu in complexMenus)
                    {
                        decimal totalPrice = menu.Products.Sum(p => p.Price);
                        decimal finalPrice = totalPrice - (totalPrice * (discountPercent / 100m));
                        finalPrice = Math.Round(finalPrice, 2);

                        string ingredientsText = "Promotional menu consisting of: " + string.Join(", ", menu.Products.Select(p => p.Name));
                        string portionsText = string.Join(" + ", menu.Products.Select(p => p.PortionQuantity));

                        var combinedAllergens = menu.Products
                            .SelectMany(p => p.Allergens ?? new List<Allergen>())
                            .GroupBy(a => a.AllergenId)
                            .Select(g => g.First())
                            .ToList();

                        string imagePath = menu.Products.FirstOrDefault(p => !string.IsNullOrEmpty(p.ImagePath))?.ImagePath ?? "/Images/default-menu.jpg";

                        decimal availableStock = menu.Products.Any() ? menu.Products.Min(p => p.TotalQuantity) : 0;

                        var menuAsProduct = new Product
                        {
                            ProductId = -menu.MenuId,
                            Name = menu.Name,
                            Price = finalPrice,
                            PortionQuantity = portionsText,
                            TotalQuantity = availableStock,
                            CategoryId = menu.CategoryId,
                            Category = menu.Category,
                            Ingredients = ingredientsText,
                            ImagePath = imagePath,
                            Allergens = combinedAllergens
                        };

                        convertedMenus.Add(menuAsProduct);
                    }

                    if (SelectedCategory != null)
                    {
                        convertedMenus = convertedMenus.Where(m => m.CategoryId == SelectedCategory.CategoryId).ToList();
                    }

                    if (!string.IsNullOrWhiteSpace(SearchKeyword))
                    {
                        convertedMenus = convertedMenus.Where(m => m.Name.Contains(SearchKeyword, StringComparison.OrdinalIgnoreCase)).ToList();
                    }

                    if (SelectedSearchAllergen != null)
                    {
                        int currentAllergenId = SelectedSearchAllergen.AllergenId;
                        if (ExcludeAllergen)
                        {
                            convertedMenus = convertedMenus.Where(m => !m.Allergens.Any(a => a.AllergenId == currentAllergenId)).ToList();
                        }
                        else
                        {
                            convertedMenus = convertedMenus.Where(m => m.Allergens.Any(a => a.AllergenId == currentAllergenId)).ToList();
                        }
                    }

                    var allItems = regularProducts.Concat(convertedMenus)
                        .OrderBy(p => p.Category?.Name ?? "")
                        .ThenBy(p => p.Name)
                        .ToList();

                    Products = new ObservableCollection<Product>(allItems);
                }
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
