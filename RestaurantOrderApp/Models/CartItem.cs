using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace RestaurantOrderApp.Models
{
    public class CartItem : Helpers.BaseViewModel
    {
        public Models.Product Product { get; set; }

        private int _quantity;
        public int Quantity
        {
            get => _quantity;
            set { _quantity = value; OnPropertyChanged(); OnPropertyChanged(nameof(TotalItemPrice)); }
        }

        public decimal TotalItemPrice => Product.Price * Quantity;
    }
}
