using System;
using System.Collections.Generic;

namespace RestaurantOrderApp.Models;

public partial class Order
{
    public int OrderId { get; set; }

    public Guid? OrderCode { get; set; }

    public int? UserId { get; set; }

    public DateTime? OrderDate { get; set; }

    public string Status { get; set; } = null!;

    public decimal? TotalCost { get; set; }

    public DateTime? EstimatedDeliveryTime { get; set; }

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual User? User { get; set; }
}
