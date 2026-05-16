using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Layers.DataAccessLayer;
using RestaurantOrderApp.Models;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Threading.Tasks;
using static RestaurantOrderApp.ViewModels.ReportsViewModel;

namespace RestaurantOrderApp.Layers.BusinessLogicLayer
{
    public class OrderBLL
    {
        private readonly OrderDAL _orderDal = new OrderDAL();

        public async Task<List<Order>> GetUserOrderHistoryAsync(int userId)
        {
            if (userId <= 0) return new List<Order>();

            using (var db = new RestaurantDbContext())
            {
                return await db.Orders
                    .Where(o => o.UserId == userId)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Orders
                    .Include(o => o.User)
                    .Include(o => o.OrderDetails)
                        .ThenInclude(od => od.Product)
                    .OrderByDescending(o => o.OrderDate)
                    .ToListAsync();
            }
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            using (var db = new RestaurantDbContext())
            {
                var dbOrder = await db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

                if (dbOrder == null) return false;

                if (dbOrder.Status != "Waiting" && dbOrder.Status != "In asteptare")
                {
                    throw new Exception("Comanda nu mai poate fi anulată deoarece este deja în preparare sau livrată.");
                }

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
                return true;
            }
        }

        public async Task ChangeStatusAsync(int orderId, string newStatus)
        {
            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateOrderStatus @OrderID = {0}, @NewStatus = {1}",
                    orderId, newStatus);
            }
        }

        public int CreateOrderFromCart(int userId, decimal totalCost, ObservableCollection<CartItem> cartItems)
        {
            if (cartItems == null || cartItems.Count == 0)
                throw new Exception("Your cart is empty.");

            int newOrderId = _orderDal.PlaceOrder(userId, totalCost, "Waiting", DateTime.Now);

            foreach (var cartItem in cartItems)
            {
                if (cartItem.Product.ProductId < 0)
                {
                    int realMenuId = -cartItem.Product.ProductId;
                    List<int> componentProductIds = _orderDal.GetProductIdsForMenu(realMenuId);

                    foreach (int productId in componentProductIds)
                    {
                        _orderDal.AddOrderDetail(newOrderId, productId, cartItem.Quantity);
                    }
                }
                else
                {
                    _orderDal.AddOrderDetail(newOrderId, cartItem.Product.ProductId, cartItem.Quantity);
                }
            }
            return newOrderId;
        }
        public bool IsCustomerLoyal(int userId, int ordersCountThreshold, int daysThreshold)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-daysThreshold);
            int recentOrdersCount = _orderDal.GetRecentOrdersCount(userId, cutoffDate);
            return recentOrdersCount > ordersCountThreshold;
        }
        public async Task<List<Order>> GetStaffOrdersBoardAsync()
        {
            return await _orderDal.GetAllOrdersWithDetailsAsync();
        }
        public async Task ChangeOrderStatusAsync(int orderId, string newStatus)
        {
            if (orderId <= 0)
                throw new ArgumentException("The order ID is invalid.");
            if (string.IsNullOrWhiteSpace(newStatus))
                throw new ArgumentException("Status cannot be empty.");

            await Task.Run(() => _orderDal.UpdateOrderStatusAsync(orderId, newStatus));
        }
        public async Task<List<Product>> GetCriticalStockReportAsync(int threshold)
        {
            if (threshold < 0)
                threshold = 0;

            return await _orderDal.GetCriticalStockFromDbAsync(threshold);
        }
        public async Task<List<ProductStats>> GetTopSellingProductsStatsAsync()
        {
            return await _orderDal.GetTopSellingProductsFromDbAsync();
        }
    }
}