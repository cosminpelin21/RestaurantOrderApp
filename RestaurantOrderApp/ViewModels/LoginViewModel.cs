using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Models;
using RestaurantOrderApp.Views;

namespace RestaurantOrderApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _email = "";
        private string _statusMessage = "";

        public string Email
        {
            get => _email;
            set
            {
                _email = value;
                OnPropertyChanged();
            }
        }

        public string StatusMessage
        {
            get => _statusMessage;
            set { _statusMessage = value; OnPropertyChanged(); }
        }

        public RelayCommand LoginCommand { get; }

        public LoginViewModel()
        {
            LoginCommand = new RelayCommand(ExecuteLogin);
        }

        private void ExecuteLogin(object? parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            string pass= passwordBox?.Password ?? "";

            if(string.IsNullOrEmpty(Email) || string.IsNullOrEmpty(pass))
            {
                StatusMessage = "Please provide your credentials.";
                return;
            }

            using (var db = new RestaurantDbContext())
            {
                var emailParam = new Microsoft.Data.SqlClient.SqlParameter("@Email", Email);
                var passParam = new Microsoft.Data.SqlClient.SqlParameter("@Password", pass);

                var user = db.Users
                    .FromSqlRaw("EXEC LoginUser @Email, @Password", emailParam, passParam)
                    .AsEnumerable()
                    .FirstOrDefault();

                if (user != null)
                {
                    UserSession.CurrentUser = user;
                    Window nextWindow=null;
                    if (user.Role == "Client")
                    {
                        nextWindow = new MenuView();
                    }
                    else if (user.Role == "Employee" || user.Role == "Admin")
                    {
                        nextWindow = new AdminView();
                    }
                    nextWindow?.Show();
                    var loginWindow = Application.Current.Windows.OfType<Window>().FirstOrDefault(w => w is LoginWindow);
                    loginWindow?.Close();
                }
                else
                {
                    StatusMessage = "Invalid username or password.";
                }
            }

        }
    }
}
