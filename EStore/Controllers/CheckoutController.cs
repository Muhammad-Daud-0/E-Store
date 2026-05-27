using System.Security.Claims;
using EStore.Models;
using EStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EStore.Controllers
{
    [Authorize]
    public class CheckoutController : Controller
    {
        private readonly IInMemoryCartService _cartService;
        private readonly AppDbContext _context;
        private readonly ILogger<CheckoutController> _logger;

        public CheckoutController(IInMemoryCartService cartService, AppDbContext context, ILogger<CheckoutController> logger)
        {
            _cartService = cartService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCart(userId);
            if (!cart.Items.Any())
            {
                return RedirectToAction("Index", "Cart");
            }

            var model = new CheckoutViewModel
            {
                Cart = cart,
                CustomerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? string.Empty
            };

            // Try to prefill checkout fields from user's profile if present
            var user = _context.Users.FirstOrDefault(u => u.Id == userId);
            if (user != null)
            {
                model.CustomerName = (user as ApplicationUser)?.FullName ?? model.CustomerName;
                model.ShippingAddress = (user as ApplicationUser)?.ShippingAddress ?? model.ShippingAddress;
                model.City = (user as ApplicationUser)?.City ?? model.City;
                model.PhoneNumber = user.PhoneNumber ?? model.PhoneNumber;
            }

            return View(model);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public async Task<IActionResult> ProcessCheckout(CheckoutViewModel model)
        {
            var userId = User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCart(userId);
            model.Cart = cart;
            model.CustomerEmail = User.FindFirst(ClaimTypes.Email)?.Value ?? User.Identity?.Name ?? string.Empty;

            if (!cart.Items.Any())
            {
                ModelState.AddModelError(string.Empty, "Your cart is empty.");
                return View("Index", model);
            }

            if (!ModelState.IsValid)
            {
                return View("Index", model);
            }

            using var transaction = await _context.Database.BeginTransactionAsync();
            try
            {
                // Re-validate prices and stock for each item against the current database state
                foreach (var item in cart.Items)
                {
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product == null)
                    {
                        ModelState.AddModelError(string.Empty, $"Product '{item.ProductName}' is no longer available.");
                        return View("Index", model);
                    }

                    if (product.StockQuantity < item.Quantity)
                    {
                        ModelState.AddModelError(string.Empty, $"Insufficient stock for product '{product.Name}'. Only {product.StockQuantity} left.");
                        return View("Index", model);
                    }

                    // Price check
                    if (Math.Abs(product.Price - item.Price) > 0.01m)
                    {
                        ModelState.AddModelError(string.Empty, $"The price of product '{product.Name}' has changed. Please review your cart.");
                        return View("Index", model);
                    }
                }

                // Create Order
                var order = new Order
                {
                    UserId = userId,
                    OrderDate = DateTime.UtcNow,
                    TotalPrice = cart.Total,
                    OrderStatus = "Pending",
                    CustomerName = model.CustomerName,
                    CustomerEmail = model.CustomerEmail,
                    ShippingAddress = model.ShippingAddress,
                    City = model.City,
                    PhoneNumber = model.PhoneNumber
                };

                _context.Orders.Add(order);
                await _context.SaveChangesAsync(); // generate order id

                // Create and add order items
                foreach (var item in cart.Items)
                {
                    var orderItem = new OrderItem
                    {
                        OrderId = order.Id,
                        ProductId = item.ProductId,
                        ProductName = item.ProductName,
                        Quantity = item.Quantity,
                        UnitPrice = item.Price
                    };
                    _context.OrderItems.Add(orderItem);

                    // Decrement stock
                    var product = await _context.Products.FindAsync(item.ProductId);
                    if (product != null)
                    {
                        product.StockQuantity -= item.Quantity;
                        _context.Products.Update(product);
                    }
                }

                await _context.SaveChangesAsync();
                await transaction.CommitAsync();

                // Clear the in-memory cart
                _cartService.ClearCart(userId);

                // Store OrderId in TempData for the success action
                TempData["OrderId"] = order.Id;

                return RedirectToAction("Success");
            }
            catch (Exception ex)
            {
                await transaction.RollbackAsync();
                _logger.LogError(ex, "Error processing checkout");
                ModelState.AddModelError(string.Empty, "An error occurred while processing your order. Please try again.");
                return View("Index", model);
            }
        }

        [HttpGet]
        public async Task<IActionResult> Success()
        {
            if (TempData["OrderId"] is int orderId)
            {
                var order = await _context.Orders
                    .Include(o => o.OrderItems)
                    .FirstOrDefaultAsync(o => o.Id == orderId);

                if (order != null)
                {
                    var viewModel = new OrderConfirmationViewModel
                    {
                        OrderId = order.Id,
                        TotalPrice = order.TotalPrice,
                        OrderDate = order.OrderDate,
                        CustomerName = order.CustomerName,
                        ShippingAddress = order.ShippingAddress,
                        City = order.City,
                        Items = order.OrderItems.ToList()
                    };

                    return View(viewModel);
                }
            }

            // Fallback if OrderId is not in TempData
            return RedirectToAction("Index", "Home");
        }
    }
}
