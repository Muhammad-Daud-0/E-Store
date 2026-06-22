using EStore.Models;
using Microsoft.EntityFrameworkCore;

namespace EStore.Services
{
    public class DatabaseCartService : ICartService
    {
        private readonly AppDbContext _context;
        private readonly ILogger<DatabaseCartService> _logger;

        public DatabaseCartService(AppDbContext context, ILogger<DatabaseCartService> logger)
        {
            _context = context;
            _logger = logger;
        }

        public ShoppingCart GetCart(string userId)
        {
            try
            {
                var cart = _context.ShoppingCarts
                    .Include(c => c.Items)
                    .FirstOrDefault(c => c.UserId == userId);

                if (cart == null)
                {
                    // Create a new cart if it doesn't exist
                    cart = new ShoppingCart
                    {
                        UserId = userId,
                        CreatedAt = DateTime.UtcNow,
                        UpdatedAt = DateTime.UtcNow,
                        Items = new List<CartItem>()
                    };
                    _context.ShoppingCarts.Add(cart);
                    _context.SaveChanges();
                }

                return cart;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cart for user {UserId}", userId);
                // Return an empty cart on error instead of throwing
                return new ShoppingCart { UserId = userId, Items = new List<CartItem>() };
            }
        }

        public void SaveCart(string userId, ShoppingCart cart)
        {
            try
            {
                cart.UserId = userId;
                cart.UpdatedAt = DateTime.UtcNow;

                var existingCart = _context.ShoppingCarts
                    .Include(c => c.Items)
                    .FirstOrDefault(c => c.UserId == userId);

                if (existingCart == null)
                {
                    _context.ShoppingCarts.Add(cart);
                }
                else
                {
                    existingCart.UpdatedAt = DateTime.UtcNow;
                    // Update items list
                    existingCart.Items = cart.Items;
                }

                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error saving cart for user {UserId}", userId);
                throw;
            }
        }

        public void AddToCart(string userId, CartItem item)
        {
            try
            {
                if (item.Quantity < 1) return;

                var cart = GetCart(userId);

                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                    _context.CartItems.Update(existingItem);
                }
                else
                {
                    item.CartId = cart.Id;
                    cart.Items.Add(item);
                    _context.CartItems.Add(item);
                }

                cart.UpdatedAt = DateTime.UtcNow;
                _context.SaveChanges();
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error adding item to cart for user {UserId}", userId);
                throw;
            }
        }

        public void RemoveFromCart(string userId, int productId)
        {
            try
            {
                var cart = GetCart(userId);
                var itemToRemove = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (itemToRemove != null)
                {
                    cart.Items.Remove(itemToRemove);
                    _context.CartItems.Remove(itemToRemove);
                    cart.UpdatedAt = DateTime.UtcNow;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error removing item from cart for user {UserId}", userId);
                throw;
            }
        }

        public void ClearCart(string userId)
        {
            try
            {
                var cart = _context.ShoppingCarts
                    .Include(c => c.Items)
                    .FirstOrDefault(c => c.UserId == userId);

                if (cart != null)
                {
                    _context.CartItems.RemoveRange(cart.Items);
                    cart.Items.Clear();
                    cart.UpdatedAt = DateTime.UtcNow;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error clearing cart for user {UserId}", userId);
                throw;
            }
        }

        public void UpdateCartItemQuantity(string userId, int productId, int quantity)
        {
            try
            {
                if (quantity <= 0)
                {
                    RemoveFromCart(userId, productId);
                    return;
                }

                var cart = GetCart(userId);
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);

                if (item != null)
                {
                    item.Quantity = quantity;
                    _context.CartItems.Update(item);
                    cart.UpdatedAt = DateTime.UtcNow;
                    _context.SaveChanges();
                }
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating cart item quantity for user {UserId}", userId);
                throw;
            }
        }
    }
}
