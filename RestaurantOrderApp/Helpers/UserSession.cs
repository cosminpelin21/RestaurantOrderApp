using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantOrderApp.Helpers
{
    public static class UserSession
    {
        public static Models.User CurrentUser { get; set; }
    }
}
