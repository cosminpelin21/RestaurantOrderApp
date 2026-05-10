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
    public partial class MyOrderView : Window
    {
        public MyOrderView()
        {
            InitializeComponent();
            this.DataContext = new MyOrdersViewModel();
        }
        private void Back_Click(object sender, RoutedEventArgs e)
        {
            var menuWin = new MenuView();
            menuWin.Show();
            this.Close();
        }
    }
}
