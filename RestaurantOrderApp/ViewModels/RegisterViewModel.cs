using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Helpers;
using Microsoft.EntityFrameworkCore;
using Microsoft.Data.SqlClient;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

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

            if (string.IsNullOrWhiteSpace(FirstName) || string.IsNullOrWhiteSpace(LastName) ||
                string.IsNullOrWhiteSpace(Email) || string.IsNullOrWhiteSpace(PhoneNumber) ||
                string.IsNullOrWhiteSpace(Address) || string.IsNullOrWhiteSpace(pass))
            {
                StatusMessage = "Please fill in all the required fields.";
                return;
            }

            using (var db = new RestaurantOrderApp.Models.RestaurantDbContext())
            {
                try
                {
                    bool exists = db.Users.Any(u => u.Email == Email);
                    if (exists)
                    {
                        StatusMessage = "This email is already registered.";
                        return;
                    }
                    var pFirstName = new Microsoft.Data.SqlClient.SqlParameter("@FirstName", FirstName);
                    var pLastName = new Microsoft.Data.SqlClient.SqlParameter("@LastName", LastName);
                    var pEmail = new Microsoft.Data.SqlClient.SqlParameter("@Email", Email);
                    var pPassword = new Microsoft.Data.SqlClient.SqlParameter("@Password", pass);
                    var pPhone = new Microsoft.Data.SqlClient.SqlParameter("@Phone", PhoneNumber);
                    var pAddress = new Microsoft.Data.SqlClient.SqlParameter("@Address", Address);
                    var pRole = new Microsoft.Data.SqlClient.SqlParameter("@Role", "Client");

                    db.Database.ExecuteSqlRaw("EXEC RegisterUser @LastName, @FirstName, @Email, @Phone, @Address,@Password, @Role",
                        pLastName,pFirstName, pEmail, pPhone, pAddress, pPassword, pRole);

                    System.Windows.MessageBox.Show("Account created successfully! Welcome to Teatris.", "Success");

                    var loginWin = new RestaurantOrderApp.Views.LoginWindow();
                    loginWin.Show();

                    System.Windows.Application.Current.Windows.OfType<System.Windows.Window>()
                        .FirstOrDefault(w => w is RestaurantOrderApp.Views.RegisterView)?.Close();
                }
                catch (System.Exception ex)
                {
                    StatusMessage = $"Error: {ex.InnerException?.Message ?? ex.Message}";
                }
            }
        }
    }
}
