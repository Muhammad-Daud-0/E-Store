using EStore.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EStore.Controllers
{
    [Authorize(Roles = "Admin")]
    public class AdminController : Controller
    {
        private readonly AppDbContext _context;
        private readonly ILogger<AdminController> _logger;
        private readonly UserManager<IdentityUser> _userManager;
        private readonly RoleManager<IdentityRole> _roleManager;

        public AdminController(
            AppDbContext context,
            ILogger<AdminController> logger,
            UserManager<IdentityUser> userManager,
            RoleManager<IdentityRole> roleManager)
        {
            _context = context;
            _logger = logger;
            _userManager = userManager;
            _roleManager = roleManager;
        }

        [HttpGet]
        public async Task<IActionResult> Dashboard(string? tab = "products")
        {
            var categories = await _context.Categories
                .Include(c => c.Products)
                .ToListAsync();
            var products = await _context.Products
                .Include(p => p.Category)
                .ToListAsync();
            var orders = await _context.Orders
                .Include(o => o.OrderItems)
                .OrderByDescending(o => o.OrderDate)
                .ToListAsync();

            var users = await _userManager.Users.ToListAsync();
            var userRoles = new Dictionary<string, string>();
            foreach (var u in users)
            {
                var roles = await _userManager.GetRolesAsync(u);
                userRoles[u.Id] = roles.FirstOrDefault() ?? "User";
            }

            var currentUserId = _userManager.GetUserId(User);
            var currentUser = users.FirstOrDefault(u => u.Id == currentUserId);

            var viewModel = new AdminDashboardViewModel
            {
                Categories = categories,
                Products = products,
                Orders = orders,
                Users = users,
                UserRoles = userRoles,
                CurrentUser = currentUser
            };

            ViewData["ActiveTab"] = tab ?? "products";
            return View(viewModel);
        }

        // Category Management Methods
        [HttpGet]
        public IActionResult AddCategory()
        {
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddCategory(Category model)
        {
            if (ModelState.IsValid)
            {
                _context.Categories.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", new { tab = "products" });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }
            return View(category);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditCategory(int id, Category model)
        {
            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Categories.AsNoTracking().FirstOrDefaultAsync(c => c.Id == id);
                    if (existing != null)
                    {
                        model.CreatedAt = existing.CreatedAt;
                    }
                    _context.Categories.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Dashboard", new { tab = "products" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Categories.Any(c => c.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteCategory(int id)
        {
            var category = await _context.Categories.FindAsync(id);
            if (category == null)
            {
                return NotFound();
            }

            var productCount = await _context.Products.CountAsync(p => p.CategoryId == id);
            if (productCount > 0)
            {
                TempData["WarningMessage"] = $"Category '{category.Name}' was deleted. {productCount} products associated with this category were also deleted.";
            }
            else
            {
                TempData["SuccessMessage"] = $"Category '{category.Name}' was deleted successfully.";
            }

            _context.Categories.Remove(category);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", new { tab = "products" });
        }

        [HttpGet]
        public async Task<IActionResult> AddProduct()
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View();
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> AddProduct(Product model)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;

            if (ModelState.IsValid)
            {
                _context.Products.Add(model);
                await _context.SaveChangesAsync();
                return RedirectToAction("Dashboard", new { tab = "products" });
            }
            return View(model);
        }

        [HttpGet]
        public async Task<IActionResult> EditProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;
            return View(product);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> EditProduct(int id, Product model)
        {
            var categories = await _context.Categories.ToListAsync();
            ViewData["Categories"] = categories;

            if (id != model.Id)
            {
                return BadRequest();
            }

            if (ModelState.IsValid)
            {
                try
                {
                    var existing = await _context.Products.AsNoTracking().FirstOrDefaultAsync(p => p.Id == id);
                    if (existing != null)
                    {
                        model.CreatedAt = existing.CreatedAt;
                    }
                    _context.Products.Update(model);
                    await _context.SaveChangesAsync();
                    return RedirectToAction("Dashboard", new { tab = "products" });
                }
                catch (DbUpdateConcurrencyException)
                {
                    if (!_context.Products.Any(p => p.Id == id))
                    {
                        return NotFound();
                    }
                    throw;
                }
            }
            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteProduct(int id)
        {
            var product = await _context.Products.FindAsync(id);
            if (product == null)
            {
                return NotFound();
            }

            _context.Products.Remove(product);
            await _context.SaveChangesAsync();
            return RedirectToAction("Dashboard", new { tab = "products" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateOrderStatus(int orderId, string status)
        {
            var order = await _context.Orders.FindAsync(orderId);
            if (order == null)
            {
                return NotFound();
            }

            if (!Enum.TryParse<OrderStatus>(status, out var parsedStatus))
            {
                return BadRequest("Invalid order status");
            }

            order.OrderStatus = parsedStatus.ToString();
            _context.Orders.Update(order);
            await _context.SaveChangesAsync();

            return RedirectToAction("Dashboard", new { tab = "orders" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> UpdateUserRole(string userId, string role)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId)
            {
                TempData["ErrorMessage"] = "You cannot change your own role.";
                return RedirectToAction("Dashboard", new { tab = "users" });
            }

            if (role != "Admin" && role != "User")
            {
                return BadRequest("Invalid role");
            }

            var currentRoles = await _userManager.GetRolesAsync(user);
            var removeResult = await _userManager.RemoveFromRolesAsync(user, currentRoles);
            if (!removeResult.Succeeded)
            {
                TempData["ErrorMessage"] = "Failed to update role.";
                return RedirectToAction("Dashboard", new { tab = "users" });
            }

            var addResult = await _userManager.AddToRoleAsync(user, role);
            if (addResult.Succeeded)
            {
                TempData["SuccessMessage"] = $"Role for {user.Email} updated to {role} successfully.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to add user to role.";
            }

            return RedirectToAction("Dashboard", new { tab = "users" });
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> DeleteUser(string userId)
        {
            var user = await _userManager.FindByIdAsync(userId);
            if (user == null)
            {
                return NotFound();
            }

            var currentUserId = _userManager.GetUserId(User);
            if (currentUserId == userId)
            {
                TempData["ErrorMessage"] = "You cannot delete your own account.";
                return RedirectToAction("Dashboard", new { tab = "users" });
            }

            var result = await _userManager.DeleteAsync(user);
            if (result.Succeeded)
            {
                TempData["SuccessMessage"] = $"User {user.Email} was successfully deleted.";
            }
            else
            {
                TempData["ErrorMessage"] = "Failed to delete user.";
            }

            return RedirectToAction("Dashboard", new { tab = "users" });
        }
    }
}
