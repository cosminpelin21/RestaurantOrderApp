using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
using RestaurantOrderApp.Models;
using RestaurantOrderApp.Views;
using System;
using System.Collections.ObjectModel;
using System.Configuration;
using System.Linq;
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class CartViewModel : BaseViewModel
    {
        private ObservableCollection<Product> _cartItems;
        private ObservableCollection<Product> _sourceItems;
        private decimal _foodCost;
        private decimal _deliveryCost;
        private decimal _totalCost;
        private decimal _discountAmount;
        private bool _isLoyalCustomer;
        private readonly OrderBLL _orderBll = new OrderBLL();

        public decimal DiscountAmount
        {
            get => _discountAmount;
            set { _discountAmount = value; OnPropertyChanged(); }
        }

        public decimal FoodCost
        {
            get => _foodCost;
            set { _foodCost = value; OnPropertyChanged(); }
        }

        public decimal DeliveryCost
        {
            get => _deliveryCost;
            set { _deliveryCost = value; OnPropertyChanged(); }
        }

        public decimal TotalCost
        {
            get => _totalCost;
            set { _totalCost = value; OnPropertyChanged(); }
        }

        private ObservableCollection<CartItem> _groupedItems;

        public ObservableCollection<CartItem> GroupedItems
        {
            get => _groupedItems;
            set { _groupedItems = value; OnPropertyChanged(); CalculateTotals(); }
        }

        public RelayCommand RemoveItemCommand { get; }
        public RelayCommand PlaceOrderCommand { get; }

        public CartViewModel(ObservableCollection<Product> items)
        {
            _sourceItems = items;
            var grouped = items.GroupBy(p => p.ProductId)
                              .Select(g => new CartItem
                              {
                                  Product = g.First(),
                                  Quantity = g.Count()
                              });
            GroupedItems = new ObservableCollection<CartItem>(grouped);
            RemoveItemCommand = new RelayCommand(ExecuteRemoveItem);
            PlaceOrderCommand = new RelayCommand(ExecutePlaceOrder, CanPlaceOrder);
            foreach (var item in GroupedItems)
            {
                item.Product.Ingredients = item.Product.Ingredients ?? "";
            }
            CalculateTotals();
            _ = CheckCustomerLoyaltyAsync();
        }

        public void CalculateTotals()
        {
            if (GroupedItems == null)
                return;

            decimal x = decimal.Parse(ConfigurationManager.AppSettings["DiscountPercent"] ?? "10");
            decimal w = decimal.Parse(ConfigurationManager.AppSettings["MinOrderForFreeDelivery"] ?? "200");
            decimal a = decimal.Parse(ConfigurationManager.AppSettings["StandardDeliveryFee"] ?? "15");
            decimal b = decimal.Parse(ConfigurationManager.AppSettings["ReducedDeliveryFee"] ?? "0");
            decimal y = decimal.Parse(ConfigurationManager.AppSettings["DiscountValueThreshold"] ?? "200");

            FoodCost = GroupedItems.Sum(i => i.TotalItemPrice);
            DeliveryCost = (FoodCost >= w) ? b : a;

            bool applyDiscount = false;

            if (FoodCost > y)
            {
                applyDiscount = true;
            }
            else if (_isLoyalCustomer)
            {
                applyDiscount = true;
            }

            if (applyDiscount)
            {
                DiscountAmount = FoodCost * (x / 100m);
            }
            else
            {
                DiscountAmount = 0;
            }

            TotalCost = FoodCost - DiscountAmount + DeliveryCost;
        }

        private void ExecuteRemoveItem(object parameter)
        {
            if (parameter is CartItem item)
            {
                GroupedItems.Remove(item);
                var toRemove = _sourceItems.Where(p => p.ProductId == item.Product.ProductId).ToList();
                foreach (var p in toRemove)
                {
                    _sourceItems.Remove(p);
                }
                CalculateTotals();
            }
        }

        private bool CanPlaceOrder(object obj) => GroupedItems != null && GroupedItems.Count > 0;

        private void ExecutePlaceOrder(object obj)
        {
            if (UserSession.CurrentUser == null)
            {
                var prompt = new RestaurantOrderApp.Views.LoginPromptView();
                bool? result = prompt.ShowDialog();
                if (result == true)
                {
                    var oldWindows = Application.Current.Windows.OfType<Window>()
                        .Where(w => w is RestaurantOrderApp.Views.MenuView || w is RestaurantOrderApp.Views.CartView)
                        .ToList();

                    var loginWin = new RestaurantOrderApp.Views.LoginWindow();
                    loginWin.ShowDialog();

                    if (UserSession.CurrentUser != null)
                    {
                        foreach (var window in oldWindows)
                        {
                            window.Close();
                        }
                    }
                }
                return;
            }
            try
            {
                int currentUserId = UserSession.CurrentUser.UserId;
                int newOrderId = _orderBll.CreateOrderFromCart(currentUserId, TotalCost, GroupedItems);

                MessageBox.Show($"Order #{newOrderId} was successfully placed!\n" +
                        $"Estimated delivery time: {DateTime.Now.AddMinutes(45):HH:mm}");

                GroupedItems.Clear();
                _sourceItems.Clear();
                CalculateTotals();
                Application.Current.Windows.OfType<CartView>().FirstOrDefault()?.Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show($"Error sending order: {ex.Message}");
            }
        }

        private async Task CheckCustomerLoyaltyAsync()
        {
            if (UserSession.CurrentUser == null) return;

            try
            {
                int z = int.Parse(ConfigurationManager.AppSettings["DiscountOrdersCountThreshold"] ?? "3");
                int t = int.Parse(ConfigurationManager.AppSettings["DiscountDaysThreshold"] ?? "30");
                int currentUserId = UserSession.CurrentUser.UserId;

                await Task.Run(() =>
                {
                    _isLoyalCustomer = _orderBll.IsCustomerLoyal(currentUserId, z, t);
                });

                CalculateTotals();
            }
            catch
            {
                _isLoyalCustomer = false;
            }
        }
    }
}