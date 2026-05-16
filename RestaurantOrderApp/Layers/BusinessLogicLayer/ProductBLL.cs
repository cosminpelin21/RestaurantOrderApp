using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using RestaurantOrderApp.Layers.DataAccessLayer;
using RestaurantOrderApp.Models;

namespace RestaurantOrderApp.Layers.BusinessLogicLayer
{
    public class ProductBLL
    {
        private readonly ProductDAL _productDal = new ProductDAL();

        public async Task<List<Product>> GetAssembledMenuAsync()
        {
            var regularProducts = await _productDal.GetRegularProductsAsync();
            var complexMenus = await _productDal.GetComplexMenusAsync();

            decimal discountPercent = 10;
            var discountSetting = System.Configuration.ConfigurationManager.AppSettings["DiscountPercent"];
            if (discountSetting != null)
            {
                decimal.TryParse(discountSetting, out discountPercent);
            }

            var convertedMenus = new List<Product>();

            foreach (var menu in complexMenus)
            {
                decimal totalPrice = menu.Products.Sum(p => p.Price);
                decimal finalPrice = totalPrice - (totalPrice * (discountPercent / 100m));
                finalPrice = Math.Round(finalPrice, 2);

                string ingredientsText = "Meniu promoțional format din: " + string.Join(", ", menu.Products.Select(p => p.Name));
                string portionsText = string.Join(" + ", menu.Products.Select(p => p.PortionQuantity));

                var combinedAllergens = menu.Products
                    .SelectMany(p => p.Allergens ?? new List<Allergen>())
                    .GroupBy(a => a.AllergenId)
                    .Select(g => g.First())
                    .ToList();

                string imagePath = menu.Products.FirstOrDefault(p => !string.IsNullOrEmpty(p.ImagePath))?.ImagePath ?? "/Images/default-menu.jpg";
                decimal availableStock = menu.Products.Any() ? menu.Products.Min(p => p.TotalQuantity) : 0;

                var menuAsProduct = new Product
                {
                    ProductId = -menu.MenuId,
                    Name = menu.Name,
                    Price = finalPrice,
                    PortionQuantity = portionsText,
                    TotalQuantity = availableStock,
                    CategoryId = menu.CategoryId,
                    Category = menu.Category,
                    Ingredients = ingredientsText,
                    ImagePath = imagePath,
                    Allergens = combinedAllergens
                };

                convertedMenus.Add(menuAsProduct);
            }

            return regularProducts.Concat(convertedMenus).ToList();
        }
        public async Task<List<Product>> GetAllProductsWithCategoryAsync()
        {
            return await _productDal.GetAllProductsWithCategoryAsync();
        }
        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            return await _productDal.GetAllCategoriesAsync();
        }
        public async Task<List<Allergen>> GetAllAllergensAsync()
        {
            return await _productDal.GetAllAllergensAsync();
        }

        public async Task<List<Category>> GetAllCategoriesOrderedAsync()
        {
            return await _productDal.GetAllCategoriesOrderedAsync();
        }

        public async Task AddProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new Exception("Product name is required.");
            if (product.Price <= 0)
                throw new Exception("The price of the product must be higher than 0.");

            await _productDal.AddProductAsync(product);
        }

        public async Task UpdateProductAsync(Product product)
        {
            if (string.IsNullOrWhiteSpace(product.Name))
                throw new Exception("Product name is required.");
            if (product.Price <= 0)
                throw new Exception("The price of the product must be higher than 0.");

            await _productDal.UpdateProductAsync(product);
        }

        public async Task DeleteProductAsync(int productId)
        {
            if (productId <= 0)
                throw new Exception("Product ID is invalid.");

            await _productDal.DeleteProductAsync(productId);
        }
        public async Task AddCategoryAsync(string categoryName)
        {
            if (string.IsNullOrWhiteSpace(categoryName))
                throw new Exception("The category name cannot be empty or consisting only of spaces.");

            var category = new Category { Name = categoryName };
            await _productDal.AddCategoryAsync(category);
        }
        public async Task DeleteCategoryAsync(int categoryId)
        {
            if (categoryId <= 0)
                throw new Exception("ID-ul categoriei este invalid.");

            bool hasProducts = await _productDal.CategoryHasProductsAsync(categoryId);
            if (hasProducts)
            {
                throw new Exception("You cannot delete this category because it contains active products! Delete products first.");
            }

            await _productDal.DeleteCategoryAsync(categoryId);
        }
        public async Task SaveProductAsync(int? productId, string name, decimal price, string portion, decimal stock, int categoryId, string ingredients, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("Numele produsului este obligatoriu.");
            if (price <= 0)
                throw new Exception("Prețul produsului trebuie să fie mai mare decât 0.");

            if (productId == null || productId == 0)
            {
                var newProd = new Product
                {
                    Name = name,
                    Price = price,
                    PortionQuantity = portion,
                    TotalQuantity = stock,
                    CategoryId = categoryId,
                    Ingredients = ingredients,
                    ImagePath = imagePath
                };
                await _productDal.AddProductAsync(newProd);
            }
            else
            {
                await _productDal.UpdateProductFieldsAsync(productId.Value, name, price, portion, stock, categoryId, ingredients, imagePath);
            }
        }
        public async Task<List<Product>> GetCriticalStockProductsAsync(decimal threshold)
        {
            if (threshold < 0)
                threshold = 0;

            return await _productDal.GetProductsBelowStockThresholdAsync(threshold);
        }

        public List<Product> GetProductsForAdmin()
        {
            return _productDal.GetAllProductsWithCategory();
        }

        public List<Category> GetCategoriesForAdmin()
        {
            return _productDal.GetAllCategories();
        }

        public void CreateProductFromAdmin(string name, decimal price, string portionQuantity, decimal totalQuantity, int categoryId, string imagePath)
        {
            if (string.IsNullOrWhiteSpace(name))
                throw new Exception("The name of the new product is mandatory.");
            if (price < 0)
                throw new Exception("The price cannot be a negative number.");

            _productDal.AddProductViaSp(name, price, portionQuantity, totalQuantity, categoryId, imagePath);
        }

        public void RemoveProductFromAdmin(int productId)
        {
            if (productId <= 0)
                throw new Exception("The selected product ID is invalid.");

            _productDal.DeleteProductViaSp(productId);
        }
    }
}