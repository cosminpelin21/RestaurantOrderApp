using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace RestaurantOrderApp.Views
{
    public partial class StaffProductsView : Window
    {
        public StaffProductsView()
        {
            InitializeComponent();
            this.DataContext = new RestaurantOrderApp.ViewModels.StaffProductsViewModel();
        }
        private void GoToOrders_Click(object sender, RoutedEventArgs e)
        {
            StaffOrdersView ordersWin = new StaffOrdersView();
            ordersWin.Show();
            this.Close();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AdminView adminWindow = new AdminView();
            adminWindow.Show();
            this.Close();
        }
    }
}
