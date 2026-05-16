using System;
using System.Linq;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Models;

namespace RestaurantOrderApp.Layers.DataAccessLayer
{
    public class UserDAL
    {
        public User LoginUser(string email, string password)
        {
            using (var db = new RestaurantDbContext())
            {
                var emailParam = new SqlParameter("@Email", email);
                var passParam = new SqlParameter("@Password", password);

                return db.Users
                    .FromSqlRaw("EXEC LoginUser @Email, @Password", emailParam, passParam)
                    .AsEnumerable()
                    .FirstOrDefault();
            }
        }

        public bool DoesEmailExist(string email)
        {
            using (var db = new RestaurantDbContext())
            {
                return db.Users.Any(u => u.Email == email);
            }
        }

        public void RegisterUser(User user, string password)
        {
            using (var db = new RestaurantDbContext())
            {
                var pFirstName = new SqlParameter("@FirstName", user.FirstName ?? "");
                var pLastName = new SqlParameter("@LastName", user.LastName ?? "");
                var pEmail = new SqlParameter("@Email", user.Email ?? "");
                var pPassword = new SqlParameter("@Password", password);
                var pPhone = new SqlParameter("@Phone", user.Phone ?? "");
                var pAddress = new SqlParameter("@Address", user.DeliveryAddress ?? "");
                var pRole = new SqlParameter("@Role", user.Role ?? "Client");

                db.Database.ExecuteSqlRaw("EXEC RegisterUser @LastName, @FirstName, @Email, @Phone, @Address, @Password, @Role",
                    pLastName, pFirstName, pEmail, pPhone, pAddress, pPassword, pRole);
            }
        }
    }
}