using System.Diagnostics;
using EStore.Models;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EStore.Controllers
{
    public class HomeController : Controller
    {
        private readonly AppDbContext _context;

        public HomeController(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IActionResult> Index(
            string? search = null,
            int? categoryId = null,
            decimal? minPrice = null,
            decimal? maxPrice = null,
            decimal? minRating = null,
            string? sort = "name",
            int page = 1,
            int pageSize = 12)
        {
            var query = _context.Products
                .Include(p => p.Category)
                .AsQueryable();

            // Search by product name
            if (!string.IsNullOrWhiteSpace(search))
            {
                var searchTerm = search.ToLower();
                query = query.Where(p => p.Name.ToLower().Contains(searchTerm) || p.Description.ToLower().Contains(searchTerm));
            }

            // Filter by category
            if (categoryId.HasValue && categoryId > 0)
            {
                query = query.Where(p => p.CategoryId == categoryId);
            }

            // Filter by price range
            if (minPrice.HasValue)
            {
                query = query.Where(p => p.Price >= minPrice);
            }
            if (maxPrice.HasValue)
            {
                query = query.Where(p => p.Price <= maxPrice);
            }

            // (reviews removed) no minRating filter

            // Apply sorting
            query = sort switch
            {
                "price-low" => query.OrderBy(p => p.Price),
                "price-high" => query.OrderByDescending(p => p.Price),
                "newest" => query.OrderByDescending(p => p.CreatedAt),
                "popular" => query.OrderByDescending(p => p.CreatedAt),
                _ => query.OrderBy(p => p.Name) // default: name
            };

            // Pagination
            var totalCount = await query.CountAsync();
            var products = await query
                .Skip((page - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            // (view count removed)

            var categories = await _context.Categories.OrderBy(c => c.Name).ToListAsync();
            var maxPriceInDb = await _context.Products.MaxAsync(p => (decimal?)p.Price) ?? 0;

            var viewModel = new HomePageViewModel
            {
                Products = products,
                Categories = categories,
                SelectedCategoryId = categoryId,
                SearchTerm = search,
                MinPrice = minPrice,
                MaxPrice = maxPrice,

                CurrentPage = page,
                PageSize = pageSize,
                TotalCount = totalCount,
                MaxPriceInDb = maxPriceInDb
            };

            ViewData["CurrentSort"] = sort;
            return View("Index", viewModel);
        }

        public IActionResult HomePage()
        {
            return View();
        }

        [HttpGet]
        public async Task<IActionResult> ProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
            {
                return NotFound();
            }

            // (view count removed)

            return View(product);
        }

        [HttpGet("api/product/{id}")]
        public async Task<IActionResult> GetProductDetails(int id)
        {
            var product = await _context.Products
                .Include(p => p.Category)
                .FirstOrDefaultAsync(p => p.Id == id);

            if (product == null)
                return NotFound();

            // Get related/recommended products
            var relatedProducts = await _context.Products
                .Where(p => p.CategoryId == product.CategoryId && p.Id != id)
                .Take(5)
                .ToListAsync();

            return Ok(new
            {
                product = new
                {
                    product.Id,
                    product.Name,
                    product.Description,
                    product.Price,
                    product.ImageUrl,
                    product.StockQuantity,
                    category = product.Category
                },
                relatedProducts
            });
        }

        public IActionResult Privacy()
        {
            return View();
        }

        [ResponseCache(Duration = 0, Location = ResponseCacheLocation.None, NoStore = true)]
        public IActionResult Error()
        {
            return View(new ErrorViewModel { RequestId = Activity.Current?.Id ?? HttpContext.TraceIdentifier });
        }
    }
}
