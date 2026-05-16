using System;
using RestaurantOrderApp.Layers.DataAccessLayer;
using RestaurantOrderApp.Models;

namespace RestaurantOrderApp.Layers.BusinessLogicLayer
{
    public class UserBLL
    {
        private readonly UserDAL _userDal = new UserDAL();

        public User ValidateAndLogin(string email, string password)
        {
            if (string.IsNullOrWhiteSpace(email) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Please provide your credentials.");
            }

            User user = _userDal.LoginUser(email, password);
            if (user == null)
            {
                throw new Exception("Invalid username or password.");
            }

            return user;
        }

        public void ValidateAndRegister(User user, string password)
        {
            if (string.IsNullOrWhiteSpace(user.FirstName) || string.IsNullOrWhiteSpace(user.LastName) ||
                string.IsNullOrWhiteSpace(user.Email) || string.IsNullOrWhiteSpace(user.Phone) ||
                string.IsNullOrWhiteSpace(user.DeliveryAddress) || string.IsNullOrWhiteSpace(password))
            {
                throw new Exception("Please fill in all the required fields.");
            }

            if (!user.Email.Contains("@"))
            {
                throw new Exception("The email format is invalid.");
            }

            if (_userDal.DoesEmailExist(user.Email))
            {
                throw new Exception("This email is already registered.");
            }

            _userDal.RegisterUser(user, password);
        }
    }
}