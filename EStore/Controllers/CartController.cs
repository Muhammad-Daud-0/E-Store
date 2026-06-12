using EStore.Models;
using EStore.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace EStore.Controllers
{
    [Authorize]
    public class CartController : Controller
    {
        private readonly ICartService _cartService;
        private readonly AppDbContext _context;
        private readonly ILogger<CartController> _logger;

        public CartController(ICartService cartService, AppDbContext context, ILogger<CartController> logger)
        {
            _cartService = cartService;
            _context = context;
            _logger = logger;
        }

        [HttpGet]
        public IActionResult Index()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var cart = _cartService.GetCart(userId);
            return View(cart);
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult AddToCart(int productId, int quantity = 1)
        {
            if (quantity < 1)
            {
                return BadRequest("Invalid quantity");
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            var product = _context.Products.Find(productId);
            if (product == null)
            {
                return NotFound();
            }

            if (product.StockQuantity < quantity)
            {
                TempData["ErrorMessage"] = $"Only {product.StockQuantity} of {product.Name} available in stock.";
                return RedirectToAction("Index");
            }

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = quantity,
                Price = product.Price,
                ImageUrl = product.ImageUrl
            };

            _cartService.AddToCart(userId, cartItem);
            TempData["SuccessMessage"] = $"{product.Name} added to cart.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult RemoveFromCart(int productId)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            _cartService.RemoveFromCart(userId, productId);
            TempData["SuccessMessage"] = "Item removed from cart.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult UpdateQuantity(int productId, int quantity)
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            if (quantity < 1)
            {
                _cartService.RemoveFromCart(userId, productId);
                TempData["SuccessMessage"] = "Item removed from cart.";
            }
            else
            {
                var product = _context.Products.Find(productId);
                if (product == null)
                {
                    return NotFound();
                }
                if (product.StockQuantity < quantity)
                {
                    TempData["ErrorMessage"] = $"Cannot update quantity. Only {product.StockQuantity} of {product.Name} available in stock.";
                    return RedirectToAction("Index");
                }
                _cartService.UpdateCartItemQuantity(userId, productId, quantity);
                TempData["SuccessMessage"] = "Cart updated.";
            }

            return RedirectToAction("Index");
        }

        [HttpPost]
        [ValidateAntiForgeryToken]
        public IActionResult ClearCart()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return RedirectToAction("Login", "Account");
            }

            _cartService.ClearCart(userId);
            TempData["SuccessMessage"] = "Cart cleared.";
            return RedirectToAction("Index");
        }

        [HttpPost]
        public IActionResult AddToCartAjax([FromBody] CartItemAddRequest request)
        {
            if (request == null || request.ProductId <= 0)
            {
                return BadRequest(new { success = false, message = "Invalid product ID" });
            }

            if (request.Quantity < 1)
            {
                return BadRequest(new { success = false, message = "Invalid quantity" });
            }

            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Unauthorized(new { success = false, message = "Please login to add items to cart" });
            }

            var product = _context.Products.Find(request.ProductId);
            if (product == null)
            {
                return NotFound(new { success = false, message = "Product not found" });
            }

            if (product.StockQuantity < request.Quantity)
            {
                return BadRequest(new { success = false, message = "Insufficient stock available" });
            }

            var cartItem = new CartItem
            {
                ProductId = product.Id,
                ProductName = product.Name,
                Quantity = request.Quantity,
                Price = product.Price,
                ImageUrl = product.ImageUrl
            };

            _cartService.AddToCart(userId, cartItem);
            var cartCount = _cartService.GetCart(userId).Items.Sum(i => i.Quantity);

            return Json(new
            {
                success = true,
                message = $"{product.Name} added to cart!",
                cartCount = cartCount
            });
        }

        [HttpGet]
        public IActionResult GetCartCount()
        {
            var userId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (string.IsNullOrEmpty(userId))
            {
                return Json(new { count = 0 });
            }

            var cartCount = _cartService.GetCart(userId).Items.Sum(i => i.Quantity);
            return Json(new { count = cartCount });
        }
    }
}
