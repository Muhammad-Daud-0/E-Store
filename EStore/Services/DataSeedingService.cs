using EStore.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using System.Text.Json;

namespace EStore.Services
{
    public class DataSeedingService
    {
        private readonly AppDbContext _dbContext;
        private readonly RoleManager<IdentityRole> _roleManager;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly ILogger<DataSeedingService> _logger;

        public DataSeedingService(
            AppDbContext dbContext,
            RoleManager<IdentityRole> roleManager,
            UserManager<IdentityUser> userManager,
            ILogger<DataSeedingService> logger)
        {
            _dbContext = dbContext;
            _roleManager = roleManager;
            _userManager = userManager;
            _logger = logger;
        }

        public async Task SeedAllDataAsync()
        {
            try
            {
                // Ensure database is created
                await _dbContext.Database.MigrateAsync();

                // Seed roles
                await SeedRolesAsync();

                // Seed users
                await SeedUsersAsync();

                // Seed categories and products
                await SeedCategoriesAndProductsAsync();
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error during data seeding: {ex.Message}");
                throw;
            }
        }

        private async Task SeedRolesAsync()
        {
            var roles = new[] { "Admin", "User" };
            foreach (var role in roles)
            {
                if (!await _roleManager.RoleExistsAsync(role))
                {
                    await _roleManager.CreateAsync(new IdentityRole(role));
                    _logger.LogInformation($"Role '{role}' created successfully.");
                }
            }
        }

        private async Task SeedUsersAsync()
        {
            // Seed admin user
            var adminEmail = "admin@store.com";
            var adminUser = await _userManager.FindByEmailAsync(adminEmail);
            if (adminUser == null)
            {
                adminUser = new IdentityUser { UserName = adminEmail, Email = adminEmail, EmailConfirmed = true };
                // WARNING: Move hardcoded password to User Secrets or environment variables for production
                var result = await _userManager.CreateAsync(adminUser, "Password123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(adminUser, "Admin");
                    _logger.LogInformation("Admin user created successfully.");
                }
            }

            // Seed regular user
            var userEmail = "user@store.com";
            var regularUser = await _userManager.FindByEmailAsync(userEmail);
            if (regularUser == null)
            {
                regularUser = new IdentityUser { UserName = userEmail, Email = userEmail, EmailConfirmed = true };
                var result = await _userManager.CreateAsync(regularUser, "Password123!");
                if (result.Succeeded)
                {
                    await _userManager.AddToRoleAsync(regularUser, "User");
                    _logger.LogInformation("Regular user created successfully.");
                }
            }
        }

        private async Task SeedCategoriesAndProductsAsync()
        {
            var seedDataPath = Path.Combine(AppContext.BaseDirectory, "seed-data.json");
            if (File.Exists(seedDataPath))
            {
                await SeedFromJsonAsync(seedDataPath);
            }
            else
            {
                await SeedFallbackDataAsync();
            }
        }

        private async Task SeedFromJsonAsync(string seedDataPath)
        {
            try
            {
                // Clear old seed data to ensure the new premium mock dataset is cleanly uploaded
                _dbContext.Products.RemoveRange(_dbContext.Products);
                _dbContext.Categories.RemoveRange(_dbContext.Categories);
                await _dbContext.SaveChangesAsync();

                // 1. Seed Parent Categories
                var parentCategories = new List<Category>
                {
                    new Category { Name = "Electronics & Gadgets", Description = "High-tech gadgets, personal computers, accessories and tablets", IconUrl = "https://cdn.dummyjson.com/product-images/laptops/apple-macbook-pro-14-inch-space-grey/thumbnail.webp", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Fashion & Apparel", Description = "Premium clothing, footwear, watches, and bags for men and women", IconUrl = "https://cdn.dummyjson.com/product-images/mens-watches/brown-leather-belt-watch/thumbnail.webp", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Home & Living", Description = "Stylish furniture, home decoration, and kitchen accessories", IconUrl = "https://cdn.dummyjson.com/product-images/furniture/annibale-colombo-bed/thumbnail.webp", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Personal Care", Description = "Beauty, fragrance, and skin care collections", IconUrl = "https://cdn.dummyjson.com/product-images/beauty/essence-mascara-lash-princess/thumbnail.webp", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Groceries & Daily Essentials", Description = "Fresh groceries, daily supplies, and pantry items", IconUrl = "https://cdn.dummyjson.com/product-images/groceries/apple/thumbnail.webp", CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Automotive & Sports", Description = "Vehicles, motorcycles, and active sports accessories", IconUrl = "https://cdn.dummyjson.com/product-images/motorcycle/generic-motorcycle/thumbnail.webp", CreatedAt = DateTime.UtcNow }
                };

                _dbContext.Categories.AddRange(parentCategories);
                await _dbContext.SaveChangesAsync();

                var parentMap = parentCategories.ToDictionary(pc => pc.Name, pc => pc.Id);

                // Subcategory to Parent category mapping
                var subCategoryMappings = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase)
                {
                    { "Laptops", "Electronics & Gadgets" },
                    { "Mobile Accessories", "Electronics & Gadgets" },
                    { "Smartphones", "Electronics & Gadgets" },
                    { "Tablets", "Electronics & Gadgets" },
                    { "Mens Shirts", "Fashion & Apparel" },
                    { "Mens Shoes", "Fashion & Apparel" },
                    { "Mens Watches", "Fashion & Apparel" },
                    { "Sunglasses", "Fashion & Apparel" },
                    { "Tops", "Fashion & Apparel" },
                    { "Womens Bags", "Fashion & Apparel" },
                    { "Womens Dresses", "Fashion & Apparel" },
                    { "Womens Jewellery", "Fashion & Apparel" },
                    { "Womens Shoes", "Fashion & Apparel" },
                    { "Womens Watches", "Fashion & Apparel" },
                    { "Furniture", "Home & Living" },
                    { "Home Decoration", "Home & Living" },
                    { "Kitchen Accessories", "Home & Living" },
                    { "Beauty", "Personal Care" },
                    { "Fragrances", "Personal Care" },
                    { "Skin Care", "Personal Care" },
                    { "Groceries", "Groceries & Daily Essentials" },
                    { "Motorcycle", "Automotive & Sports" },
                    { "Vehicle", "Automotive & Sports" },
                    { "Sports Accessories", "Automotive & Sports" }
                };

                var jsonContent = await File.ReadAllTextAsync(seedDataPath);
                using (JsonDocument doc = JsonDocument.Parse(jsonContent))
                {
                    var root = doc.RootElement;

                    // Seed subcategories under parents
                    var categoriesArray = root.GetProperty("categories");
                    var categoryMap = new Dictionary<string, int>();

                    foreach (var categoryElement in categoriesArray.EnumerateArray())
                    {
                        var categoryName = categoryElement.GetProperty("name").GetString() ?? string.Empty;
                        var categoryDescription = categoryElement.GetProperty("description").GetString() ?? string.Empty;
                        var categoryIconUrl = categoryElement.GetProperty("iconUrl").GetString() ?? string.Empty;

                        int? parentId = null;
                        if (subCategoryMappings.TryGetValue(categoryName, out var parentName) && parentMap.TryGetValue(parentName, out var pId))
                        {
                            parentId = pId;
                        }

                        var category = new Category
                        {
                            Name = categoryName,
                            Description = categoryDescription,
                            IconUrl = categoryIconUrl,
                            ParentCategoryId = parentId,
                            CreatedAt = DateTime.UtcNow
                        };

                        _dbContext.Categories.Add(category);
                        await _dbContext.SaveChangesAsync();
                        categoryMap[categoryName] = category.Id;
                    }

                    // Seed products
                    var productsArray = root.GetProperty("products");
                    foreach (var productElement in productsArray.EnumerateArray())
                    {
                        var productName = productElement.GetProperty("name").GetString();
                        var productDescription = productElement.GetProperty("description").GetString();
                        var productPrice = productElement.GetProperty("price").GetDecimal();
                        var productImageUrl = productElement.GetProperty("imageUrl").GetString();
                        var productStock = productElement.GetProperty("stockQuantity").GetInt32();
                        var productCategoryName = productElement.GetProperty("categoryName").GetString();

                        if (categoryMap.TryGetValue(productCategoryName ?? "", out var categoryId))
                        {
                            var product = new Product
                            {
                                Name = productName ?? string.Empty,
                                Description = productDescription ?? string.Empty,
                                Price = productPrice,
                                ImageUrl = productImageUrl ?? string.Empty,
                                StockQuantity = productStock,
                                CategoryId = categoryId,
                                CreatedAt = DateTime.UtcNow
                            };

                            _dbContext.Products.Add(product);
                        }
                    }

                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Categories and products seeded from JSON successfully.");
                }
            }
            catch (Exception ex)
            {
                _logger.LogError($"Error seeding data from JSON: {ex.Message}");
                throw;
            }
        }

        private async Task SeedFallbackDataAsync()
        {
            // Fallback: seed products with categories inline if JSON file not found
            if (!_dbContext.Categories.Any())
            {
                var parent = new Category { Name = "Electronics & Gadgets", Description = "High-tech gadgets, personal computers, accessories and tablets", IconUrl = "https://images.unsplash.com/photo-1550355291-bbee04a92027?w=100", CreatedAt = DateTime.UtcNow };
                _dbContext.Categories.Add(parent);
                await _dbContext.SaveChangesAsync();

                var subCategories = new[]
                {
                    new Category { Name = "Electronics", Description = "High-tech gadgets", IconUrl = "https://images.unsplash.com/photo-1550355291-bbee04a92027?w=100", ParentCategoryId = parent.Id, CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Office Equipment", Description = "Professional office furniture", IconUrl = "https://images.unsplash.com/photo-1593642632823-8f785ba67e45?w=100", ParentCategoryId = parent.Id, CreatedAt = DateTime.UtcNow },
                    new Category { Name = "Audio & Video", Description = "Premium audio and video equipment", IconUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=100", ParentCategoryId = parent.Id, CreatedAt = DateTime.UtcNow }
                };

                _dbContext.Categories.AddRange(subCategories);
                await _dbContext.SaveChangesAsync();
            }

            if (!_dbContext.Products.Any())
            {
                var electronicsCategory = _dbContext.Categories.FirstOrDefault(c => c.Name == "Electronics");
                var officeCategory = _dbContext.Categories.FirstOrDefault(c => c.Name == "Office Equipment");
                var audioCategory = _dbContext.Categories.FirstOrDefault(c => c.Name == "Audio & Video");

                var products = new List<Product>();

                if (electronicsCategory != null)
                {
                    products.Add(new Product
                    {
                        Name = "Premium Wireless Keyboard",
                        Description = "High-performance wireless keyboard with mechanical switches",
                        Price = 129.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1587829191301-72e0f0c57114?w=400",
                        StockQuantity = 50,
                        CategoryId = electronicsCategory.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                    products.Add(new Product
                    {
                        Name = "Ultra HD 4K Monitor",
                        Description = "27-inch 4K IPS display with HDR support",
                        Price = 599.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1527864550417-7fd91fc51a46?w=400",
                        StockQuantity = 30,
                        CategoryId = electronicsCategory.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (officeCategory != null)
                {
                    products.Add(new Product
                    {
                        Name = "Ergonomic Office Chair",
                        Description = "Premium ergonomic chair with lumbar support",
                        Price = 349.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1588195538326-c5b1e6f4e799?w=400",
                        StockQuantity = 25,
                        CategoryId = officeCategory.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (audioCategory != null)
                {
                    products.Add(new Product
                    {
                        Name = "Studio Audio Headphones",
                        Description = "Professional-grade studio headphones with noise cancellation",
                        Price = 249.99m,
                        ImageUrl = "https://images.unsplash.com/photo-1505740420928-5e560c06d30e?w=400",
                        StockQuantity = 40,
                        CategoryId = audioCategory.Id,
                        CreatedAt = DateTime.UtcNow
                    });
                }

                if (products.Any())
                {
                    _dbContext.Products.AddRange(products);
                    await _dbContext.SaveChangesAsync();
                    _logger.LogInformation("Fallback categories and products seeded successfully.");
                }
            }
        }
    }
}
