using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class StaffOrdersViewModel : BaseViewModel
    {
        private ObservableCollection<Order> _allOrders;
        public ObservableCollection<Order> AllOrders
        {
            get => _allOrders;
            set { _allOrders = value; OnPropertyChanged(); }
        }

        public RelayCommand PrepareOrderCommand { get; }
        public RelayCommand DeliverOrderCommand { get; }
        public RelayCommand CancelOrderCommand { get; }

        public StaffOrdersViewModel()
        {
            PrepareOrderCommand = new RelayCommand(async p => await ChangeStatusAsync(p as Order, "In preparation"));
            DeliverOrderCommand = new RelayCommand(async p => await ChangeStatusAsync(p as Order, "Delivered"));
            CancelOrderCommand = new RelayCommand(async p => await CancelOrderAsync(p as Order));

            _ = LoadOrdersAsync();
        }

        public async Task LoadOrdersAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                var orders = await db.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();

                AllOrders = new ObservableCollection<Order>(orders);
            }
        }

        private async Task ChangeStatusAsync(Order order, string newStatus)
        {
            if (order == null) return;

            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateOrderStatus @OrderID = {0}, @NewStatus = {1}",
                    order.OrderId, newStatus);
            }

            await LoadOrdersAsync();
        }

        private async Task CancelOrderAsync(Order order)
        {
            if (order == null) return;

            var result = MessageBox.Show($"Are you sure you want to cancel order {order.OrderCode}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            using (var db = new RestaurantDbContext())
            {
                var dbOrder = await db.Orders.Include(o => o.OrderDetails).FirstOrDefaultAsync(o => o.OrderId == order.OrderId);
                if (dbOrder != null)
                {
                    dbOrder.Status = "Cancelled";

                    foreach (var detail in dbOrder.OrderDetails)
                    {
                        var product = await db.Products.FindAsync(detail.ProductId);
                        if (product != null) product.TotalQuantity += detail.Quantity;
                    }
                    await db.SaveChangesAsync();
                }
            }
            await LoadOrdersAsync();
        }
    }
}