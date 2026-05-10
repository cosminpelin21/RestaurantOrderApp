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
    public class MyOrdersViewModel : BaseViewModel
    {
        private ObservableCollection<Order> _userOrders;
        public ObservableCollection<Order> UserOrders
        {
            get => _userOrders;
            set { _userOrders = value; OnPropertyChanged(); }
        }

        public RelayCommand CancelOrderCommand { get; }

        public MyOrdersViewModel()
        {
            CancelOrderCommand = new RelayCommand(ExecuteCancelOrder);
            _ = LoadOrdersAsync();
        }

        public async Task LoadOrdersAsync()
        {
            if (UserSession.CurrentUser == null)
            {
                System.Windows.MessageBox.Show("Error: You are not logged in!");
                return;
            }

            try
            {
                using (var db = new RestaurantDbContext())
                {
                    var orders = await db.Orders
                        .Where(o => o.UserId == UserSession.CurrentUser.UserId)
                        .Include(o => o.OrderDetails)
                            .ThenInclude(od => od.Product)
                        .OrderByDescending(o => o.OrderDate)
                        .ToListAsync();

                    UserOrders = new ObservableCollection<Order>(orders);

                    if (UserOrders.Count == 0)
                    {
                        System.Diagnostics.Debug.WriteLine("No commands found for this user.");
                    }
                }
            }
            catch (Exception ex)
            {
                System.Windows.MessageBox.Show("Error loading commands: " + ex.Message);
            }
        }

        private async void ExecuteCancelOrder(object parameter)
        {
            if (parameter is Order order && order.Status == "Waiting")
            {
                using (var db = new RestaurantDbContext())
                {
                    var dbOrder = await db.Orders
                        .Include(o => o.OrderDetails)
                        .FirstOrDefaultAsync(o => o.OrderId == order.OrderId);

                    if (dbOrder != null)
                    {
                        dbOrder.Status = "Cancelled";

                        foreach (var detail in dbOrder.OrderDetails)
                        {
                            var product = await db.Products.FindAsync(detail.ProductId);
                            if (product != null)
                            {
                                product.TotalQuantity += detail.Quantity;
                            }
                        }

                        await db.SaveChangesAsync();
                    }
                }

                await LoadOrdersAsync();
            }
        }
    }

}
