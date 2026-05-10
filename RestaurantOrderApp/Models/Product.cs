using System;
using System.Collections.Generic;

namespace RestaurantOrderApp.Models;

public partial class Product
{
    public int ProductId { get; set; }

    public string Name { get; set; } = null!;

    public decimal Price { get; set; }

    public string Ingredients { get; set; } = string.Empty;

    public string PortionQuantity { get; set; } = null!;

    public decimal TotalQuantity { get; set; }

    public int CategoryId { get; set; }

    public string? ImagePath { get; set; }

    public virtual Category Category { get; set; } = null!;

    public virtual ICollection<OrderDetail> OrderDetails { get; set; } = new List<OrderDetail>();

    public virtual ICollection<Allergen> Allergens { get; set; } = new List<Allergen>();

    public virtual ICollection<Menu> Menus { get; set; } = new List<Menu>();
    public string AllergensList => Allergens != null && Allergens.Any()
        ? string.Join(", ", Allergens.Select(a => a.Name))
        : "No common allergens";
    public string FullDescription =>
        $"Ingredients: {Ingredients}\nAllergens: {(Allergens != null && Allergens.Any() ? string.Join(", ", Allergens.Select(a => a.Name)) : "None")}";
}
