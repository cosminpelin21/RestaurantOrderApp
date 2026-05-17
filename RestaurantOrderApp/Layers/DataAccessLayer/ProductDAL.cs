using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using Microsoft.EntityFrameworkCore;
using RestaurantOrderApp.Models;

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
                return await db.Products
                    .Include(p => p.Category)
                    .Where(p => p.TotalQuantity <= threshold)
                    .OrderBy(p => p.TotalQuantity)
                    .ToListAsync();
            }
        }

        public async Task AddProductAsync(Product product)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Products.Add(product);
                await db.SaveChangesAsync();
            }
        }

        public async Task UpdateProductFieldsAsync(int productId, string name, decimal price, string portion, decimal stock, int categoryId, string ingredients, string imagePath)
        {
            using (var db = new RestaurantDbContext())
            {
                var p = await db.Products.FindAsync(productId);
                if (p != null)
                {
                    p.Name = name;
                    p.Price = price;
                    p.PortionQuantity = portion;
                    p.TotalQuantity = stock;
                    p.CategoryId = categoryId;
                    p.Ingredients = ingredients;
                    p.ImagePath = imagePath;

                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task UpdateProductAsync(Product product)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Entry(product).State = EntityState.Modified;
                await db.SaveChangesAsync();
            }
        }

        public async Task DeleteProductAsync(int productId)
        {
            using (var db = new RestaurantDbContext())
            {
                var product = await db.Products.FindAsync(productId);
                if (product != null)
                {
                    db.Products.Remove(product);
                    await db.SaveChangesAsync();
                }
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
                var category = await db.Categories.FindAsync(categoryId);
                if (category != null)
                {
                    db.Categories.Remove(category);
                    await db.SaveChangesAsync();
                }
            }
        }

        public async Task<List<Allergen>> GetAllAllergensAsync()
        {
            using (var db = new RestaurantDbContext())
            {
                return await db.Allergens.OrderBy(a => a.Name).ToListAsync();
            }
        }

        public List<Product> GetAllProductsWithCategory()
        {
            using (var db = new RestaurantDbContext())
            {
                return db.Products.Include(p => p.Category).ToList();
            }
        }

        public List<Category> GetAllCategories()
        {
            using (var db = new RestaurantDbContext())
            {
                return db.Categories.ToList();
            }
        }

        public void AddProductViaSp(string name, decimal price, string portionQuantity, decimal totalQuantity, int categoryId, string imagePath)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Database.ExecuteSqlRaw("EXEC AddProduct @Name={0}, @Price={1}, @PortionQuantity={2}, @TotalQuantity={3}, @CategoryId={4}, @ImagePath={5}",
                    name, price, portionQuantity, totalQuantity, categoryId, imagePath);
            }
        }

        public void DeleteProductViaSp(int productId)
        {
            using (var db = new RestaurantDbContext())
            {
                db.Database.ExecuteSqlRaw("EXEC DeleteProduct @ProductId={0}", productId);
            }
        }
    }
}