using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOrderApp.Layers.DataAccessLayer
{
    public class OrderDAL
    {
        public int PlaceOrder(int userId, decimal totalCost, string status, DateTime orderDate)
        {
            using (var db = new RestaurantDbContext())
            {
                var result = db.Database.SqlQueryRaw<decimal>(
                    "EXEC PlaceOrder @UserId={0}, @TotalCost={1}, @Status={2}, @OrderDate={3}",
                    userId, totalCost, status, orderDate).AsEnumerable().FirstOrDefault();

                return Convert.ToInt32(result);
            }
        }

        public void AddOrderDetail(int orderId, int productId, int quantity)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Database.ExecuteSqlRaw(
                    "EXEC AddOrderDetail @OrderId={0}, @ProductId={1}, @Quantity={2}",
                    orderId, productId, quantity);
            }
        }

        public List<int> GetProductIdsForMenu(int menuId)
        {
            using (var db = new RestaurantDbContext())
            {
                return db.Menus
                    .Where(m => m.MenuId == menuId)
                    .SelectMany(m => m.Products.Select(p => p.ProductId))
                    .ToList();
            }
        }

        public int GetRecentOrdersCount(int userId, DateTime cutoffDate)
        {
            using (var db = new RestaurantDbContext())
            {
                return db.Orders.Count(o => o.UserId == userId && o.OrderDate >= cutoffDate);
            }
        }

        public async Task<List<Order>> GetOrdersByUserIdAsync(int userId)
        {
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

        public async Task<Order> GetOrderByIdAsync(int orderId)
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);
            }
        }

        public async Task ExecuteCancelAndRestoreStockAsync(int orderId)
        {
            using (var db = new RestaurantDbContext())
            {
                var dbOrder = await db.Orders
                    .Include(o => o.OrderDetails)
                    .FirstOrDefaultAsync(o => o.OrderId == orderId);

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
        }

        public async Task UpdateOrderStatusAsync(int orderId, string newStatus)
        {
            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync(
                    "EXEC UpdateOrderStatus @OrderID={0}, @NewStatus={1}",
                    orderId, newStatus);
            }
        }

        public async Task<List<Order>> GetAllOrdersWithDetailsAsync()
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

    }
}