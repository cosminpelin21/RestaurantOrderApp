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
    public partial class StaffReportsView : Window
    {
        public StaffReportsView()
        {
            InitializeComponent();
            DataContext = new ViewModels.StaffReportsViewModel();
        }

        private void Back_Click(object sender, RoutedEventArgs e)
        {
            new AdminView().Show();
            this.Close();
        }
    }
}
