using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
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
    public class RegisterViewModel : BaseViewModel
    {
        private string _firstName = "";
        private string _lastName = "";
        private string _email = "";
        private string _phoneNumber = "";
        private string _address = "";
        private string _statusMessage = "";
        private readonly UserBLL _userBll = new UserBLL();

        public string FirstName { get => _firstName; set { _firstName = value; OnPropertyChanged(); } }
        public string LastName { get => _lastName; set { _lastName = value; OnPropertyChanged(); } }
        public string Email { get => _email; set { _email = value; OnPropertyChanged(); } }
        public string PhoneNumber { get => _phoneNumber; set { _phoneNumber = value; OnPropertyChanged(); } }
        public string Address { get => _address; set { _address = value; OnPropertyChanged(); } }
        public string StatusMessage { get => _statusMessage; set { _statusMessage = value; OnPropertyChanged(); } }

        public RelayCommand RegisterCommand { get; }
        public RegisterViewModel()
        {
            RegisterCommand = new RelayCommand(ExecuteRegister);
        }
        private void ExecuteRegister(object? parameter)
        {
            var passwordBox = parameter as System.Windows.Controls.PasswordBox;
            string pass = passwordBox?.Password ?? "";

            try
            {
                var newUser = new User
                {
                    FirstName = FirstName,
                    LastName = LastName,
                    Email = Email,
                    Phone = PhoneNumber,
                    DeliveryAddress = Address,
                    Role = "Client"
                };

                _userBll.ValidateAndRegister(newUser, pass);

                MessageBox.Show("Account created successfully! Welcome to Teatris.", "Success");

                var loginWin = new LoginWindow();
                loginWin.Show();

                Application.Current.Windows.OfType<Window>()
                    .FirstOrDefault(w => w is RegisterView)?.Close();
            }
            catch (Exception ex)
            {
                StatusMessage = ex.Message;
            }
        }
    }
}
