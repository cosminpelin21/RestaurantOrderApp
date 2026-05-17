using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace RestaurantOrderApp.Layers.DataAccessLayer
{
    public class ProductDAL
    {
        public async Task<List<Product>> GetRegularProductsAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Products
                    .Include(p => p.Category)
                    .Include(p => p.Allergens)
                    .ToListAsync();
            }
        }

        public async Task<List<Menu>> GetComplexMenusAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Menus
                    .Include(m => m.Category)
                    .Include(m => m.Products)
                        .ThenInclude(p => p.Allergens)
                    .ToListAsync();
            }
        }

        public async Task<List<Product>> GetAllProductsWithCategoryAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Products.Include(p => p.Category).ToListAsync();
            }
        }

        public async Task<List<Product>> GetProductsBelowStockThresholdAsync(decimal threshold)
        {
            using (var db = new RestaurantDbContext())
            {
                var thresholdParam = new SqlParameter("@Threshold", Convert.ToInt32(threshold));
                return await db.Products
                    .FromSqlRaw("EXEC GetCriticalStock @Threshold", thresholdParam)
                    .ToListAsync();
            }
        }

        public async Task AddProductAsync(Product product)
        {
            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync(
                    "EXEC AddProduct @Name={0}, @CategoryID={1}, @Price={2}, @TotalQuantity={3}, @PortionQuantity={4}, @Ingredients={5}",
                    product.Name, product.CategoryId, product.Price, product.TotalQuantity, product.PortionQuantity, product.Ingredients);
            }
        }

        public async Task UpdateProductFieldsAsync(int productId, string name, decimal price, string portion, decimal stock, int categoryId, string ingredients, string imagePath)
        {
            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync(
             "EXEC UpdateProductDetails @ProductID={0}, @Name={1}, @Price={2}, @PortionQuantity={3}, @TotalQuantity={4}, @CategoryID={5}, @Ingredients={6}, @ImagePath={7}",
             productId, name, price, portion, stock, categoryId, ingredients, imagePath);
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync("EXEC DeleteProduct @ProductId={0}", productId);
            }
        }

        public async Task<List<Category>> GetAllCategoriesAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Categories.ToListAsync();
            }
        }

        public async Task<List<Category>> GetAllCategoriesOrderedAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Categories.OrderBy(c => c.Name).ToListAsync();
            }
        }

        public async Task AddCategoryAsync(Category category)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Categories.Add(category);
                await db.SaveChangesAsync();
            }
        }

        public async Task<bool> CategoryHasProductsAsync(int categoryId)
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Products.AnyAsync(p => p.CategoryId == categoryId);
            }
        }

        public async Task DeleteCategoryAsync(int categoryId)
        {
            using (var db = new RestaurantDbContext())
            {
                await db.Database.ExecuteSqlRawAsync("EXEC DeleteCategory @CategoryID={0}", categoryId);
            }
        }

        public async Task<List<Allergen>> GetAllAllergensAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Allergens.OrderBy(a => a.Name).ToListAsync();
            }
        }
    }
}