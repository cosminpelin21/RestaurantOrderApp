using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
using RestaurantOrderApp.Models;
using System.Collections.ObjectModel;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class StaffOrdersViewModel : BaseViewModel
    {
        private readonly OrderBLL _orderBll = new OrderBLL();
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
            try
            {
                var orders = await _orderBll.GetAllOrdersAsync();
                AllOrders = new ObservableCollection<Order>(orders);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error loading all orders: " + ex.Message);
            }
        }

        private async Task ChangeStatusAsync(Order order, string newStatus)
        {
            if (order == null) return;

            try
            {
                await _orderBll.ChangeStatusAsync(order.OrderId, newStatus);
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Error changing status: " + ex.Message);
            }
        }

        private async Task CancelOrderAsync(Order order)
        {
            if (order == null) return;

            var result = MessageBox.Show($"Are you sure you want to cancel order {order.OrderCode}?", "Confirmation", MessageBoxButton.YesNo, MessageBoxImage.Warning);
            if (result != MessageBoxResult.Yes) return;

            try
            {
                await _orderBll.CancelOrderAsync(order.OrderId);
                MessageBox.Show("The order has been canceled by the staff!");
                await LoadOrdersAsync();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Could not cancel order: " + ex.Message);
            }
        }
    }
}