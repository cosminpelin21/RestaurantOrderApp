using System;
using System.Collections.Generic;

namespace RestaurantOrderApp.Models;

public partial class OrderDetail
{
    public int OrderDetailId { get; set; }

    public int? OrderId { get; set; }

    public int? ProductId { get; set; }

    public int? MenuId { get; set; }

    public int Quantity { get; set; }

    public virtual Menu? Menu { get; set; }

    public virtual Order? Order { get; set; }

    public virtual Product? Product { get; set; }
}
