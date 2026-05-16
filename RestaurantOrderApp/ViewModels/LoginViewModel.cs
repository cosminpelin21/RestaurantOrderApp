using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using RestaurantOrderApp.Layers.BusinessLogicLayer;
using RestaurantOrderApp.Models;
using RestaurantOrderApp.Views;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace RestaurantOrderApp.ViewModels
{
    public class LoginViewModel : BaseViewModel
    {
        private string _email = "";
        private string _statusMessage = "";
        private readonly UserBLL _userBll = new UserBLL();

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
            string pass = passwordBox?.Password ?? "";

            try
            {
                User user = _userBll.ValidateAndLogin(Email, pass);

                UserSession.CurrentUser = user;
                Window nextWindow = null;

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
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
    }
}
