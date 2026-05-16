using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
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
    public class MyOrdersViewModel : BaseViewModel
    {
        private ObservableCollection<Order> _userOrders;
        private readonly OrderBLL _orderBll = new OrderBLL();
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
                var orders = await _orderBll.GetUserOrderHistoryAsync(UserSession.CurrentUser.UserId);
                UserOrders = new ObservableCollection<Order>(orders);
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
                try
                {
                    bool success = await _orderBll.CancelOrderAsync(order.OrderId);
                    if (success)
                    {
                        MessageBox.Show("Order successfully canceled!");
                        await LoadOrdersAsync();
                    }
                }
                catch (Exception ex)
                {
                    MessageBox.Show(ex.Message, "Unsuccessful cancellation", MessageBoxButton.OK, MessageBoxImage.Warning);
                }
            }
        }
    }

}
