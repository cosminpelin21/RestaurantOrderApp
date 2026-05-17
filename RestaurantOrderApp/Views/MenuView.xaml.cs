using RestaurantOrderApp.ViewModels;
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
    public partial class MenuView : Window
    {
        public MenuView()
        {
            InitializeComponent();
            var vm = new MenuViewModel();
            this.DataContext = vm;
            this.Loaded += async (s, e) =>
            {
                await vm.InitializeAsync();
            };
        }

        private void Window_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Escape)
            {
                Application.Current.Shutdown();
            }
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var startWin = new StartWindow();
            startWin.Show();
            this.Close();
        }
        //private void OpenLogin_Click(object sender, RoutedEventArgs e)
        //{
        //    var oldWindows = Application.Current.Windows.OfType<Window>()
        //        .Where(w => w is RestaurantOrderApp.Views.MenuView || w is RestaurantOrderApp.Views.CartView)
        //        .ToList();
        //    var loginWin = new LoginWindow();
        //    loginWin.WindowStyle = WindowStyle.None;
        //    loginWin.WindowState = WindowState.Maximized;
        //    loginWin.ShowDialog();
        //    if (this.DataContext is MenuViewModel vm)
        //    {
        //        vm.RefreshLoginStatus();
        //    }
        //    if (RestaurantOrderApp.Helpers.UserSession.CurrentUser != null)
        //    {
        //        foreach (var window in oldWindows)
        //        {
        //            window.Close();
        //        }
        //    }
        //}
        private void OpenOrders_Click(object sender, RoutedEventArgs e)
        {
            var historyWin = new MyOrderView();
            historyWin.Show();
        }
    }
}
