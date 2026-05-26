using EStore.Models;

namespace EStore.Services
{
    public interface IInMemoryCartService
    {
        ShoppingCart GetCart(string userId);
        void SaveCart(string userId, ShoppingCart cart);
        void AddToCart(string userId, CartItem item);
        void RemoveFromCart(string userId, int productId);
        void ClearCart(string userId);
        void UpdateCartItemQuantity(string userId, int productId, int quantity);
    }

    public class InMemoryCartService : IInMemoryCartService
    {
        // Static concurrent dictionary to store carts thread-safely in memory
        private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, ShoppingCart> _carts = new();

        public ShoppingCart GetCart(string userId)
        {
            return _carts.GetOrAdd(userId, id => new ShoppingCart { UserId = id });
        }

        public void SaveCart(string userId, ShoppingCart cart)
        {
            cart.UserId = userId;
            _carts[userId] = cart;
        }

        public void AddToCart(string userId, CartItem item)
        {
            if (item.Quantity < 1) return;

            var cart = GetCart(userId);
            lock (cart)
            {
                var existingItem = cart.Items.FirstOrDefault(i => i.ProductId == item.ProductId);
                if (existingItem != null)
                {
                    existingItem.Quantity += item.Quantity;
                }
                else
                {
                    cart.Items.Add(item);
                }
            }
        }

        public void RemoveFromCart(string userId, int productId)
        {
            var cart = GetCart(userId);
            lock (cart)
            {
                cart.Items = cart.Items.Where(i => i.ProductId != productId).ToList();
            }
        }

        public void ClearCart(string userId)
        {
            _carts.TryRemove(userId, out _);
        }

        public void UpdateCartItemQuantity(string userId, int productId, int quantity)
        {
            if (quantity <= 0)
            {
                RemoveFromCart(userId, productId);
                return;
            }

            var cart = GetCart(userId);
            lock (cart)
            {
                var item = cart.Items.FirstOrDefault(i => i.ProductId == productId);
                if (item != null)
                {
                    item.Quantity = quantity;
                }
            }
        }
    }
}
