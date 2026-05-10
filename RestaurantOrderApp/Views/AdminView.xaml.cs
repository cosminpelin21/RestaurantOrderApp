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
    public partial class AdminView : Window
    {
        public AdminView()
        {
            InitializeComponent();
        }
        private void GoToOrders_Click(object sender, RoutedEventArgs e)
        {
            new StaffOrdersView().Show();
            this.Close();
        }

        private void GoToProducts_Click(object sender, RoutedEventArgs e)
        {
            new StaffProductsView().Show();
            this.Close();
        }

        private void GoToReports_Click(object sender, RoutedEventArgs e)
        {
            StaffReportsView reportsWin = new StaffReportsView();
            reportsWin.Show();
            this.Close();
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            new LoginWindow().Show();
            this.Close();
        }
    }
}
