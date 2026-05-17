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

        public async Task<List<Order>> GetUserOrderHistoryAsync(int userId)
        {
            if (userId <= 0) return new List<Order>();
            return await _orderDal.GetOrdersByUserIdAsync(userId);
        }

        public bool IsCustomerLoyal(int userId, int ordersCountThreshold, int daysThreshold)
        {
            DateTime cutoffDate = DateTime.Now.AddDays(-daysThreshold);
            int recentOrdersCount = _orderDal.GetRecentOrdersCount(userId, cutoffDate);
            return recentOrdersCount > ordersCountThreshold;
        }

        public async Task<List<Order>> GetAllOrdersAsync()
        {
            return await _orderDal.GetAllOrdersWithDetailsAsync();
        }

        public async Task<List<Order>> GetStaffOrdersBoardAsync()
        {
            return await _orderDal.GetAllOrdersWithDetailsAsync();
        }

        public async Task ChangeStatusAsync(int orderId, string newStatus)
        {
            if (orderId <= 0) throw new ArgumentException("ID-ul comenzii este invalid.");
            if (string.IsNullOrWhiteSpace(newStatus)) throw new ArgumentException("Statusul nu poate fi gol.");

            await _orderDal.UpdateOrderStatusAsync(orderId, newStatus);
        }

        public async Task<bool> CancelOrderAsync(int orderId)
        {
            var dbOrder = await _orderDal.GetOrderByIdAsync(orderId);
            if (dbOrder == null) return false;

            if (dbOrder.Status != "Waiting" && dbOrder.Status != "In asteptare")
            {
                throw new Exception("Comanda nu mai poate fi anulată deoarece este deja în preparare sau livrată.");
            }

            await _orderDal.ExecuteCancelAndRestoreStockAsync(orderId);
            return true;
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