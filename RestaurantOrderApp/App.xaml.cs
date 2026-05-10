using System.Configuration;
using System.Data;
using System.Windows;

namespace RestaurantOrderApp
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            System.Threading.Tasks.Task.Run(() => {
                using (var db = new RestaurantOrderApp.Models.RestaurantDbContext())
                {
                    var dummy = db.Products.Any();
                }
            });
        }
    }

}
