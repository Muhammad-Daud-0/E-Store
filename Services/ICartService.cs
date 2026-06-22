using EStore.Models;

namespace EStore.Services
{
    public interface ICartService
    {
        ShoppingCart GetCart(string userId);
        void SaveCart(string userId, ShoppingCart cart);
        void AddToCart(string userId, CartItem item);
        void RemoveFromCart(string userId, int productId);
        void ClearCart(string userId);
        void UpdateCartItemQuantity(string userId, int productId, int quantity);
    }
}

