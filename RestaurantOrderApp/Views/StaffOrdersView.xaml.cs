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
    public partial class StaffOrdersView : Window
    {
        public StaffOrdersView()
        {
            InitializeComponent();
            this.DataContext = new ViewModels.StaffOrdersViewModel();
        }
        private void OpenReports_Click(object sender, RoutedEventArgs e)
        {
            var reportsWin = new ReportsView();
            reportsWin.Show();
        }
        private void OpenStaffMenu_Click(object sender, RoutedEventArgs e)
        {
            new StaffProductsView().Show();
        }
        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            var startWin = new StartWindow();
            startWin.WindowStyle = WindowStyle.None;
            startWin.WindowState = WindowState.Maximized;
            startWin.ShowDialog();
            this.Close();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            AdminView adminWindow = new AdminView();
            adminWindow.Show();
            this.Close();
        }
        private void GoToProducts_Click(object sender, RoutedEventArgs e)
        {
            StaffProductsView productsWin = new StaffProductsView();
            productsWin.Show();
            this.Close();
        }
    }
}
