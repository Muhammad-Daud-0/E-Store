using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace EStore.Models
{
    public class CartItem
    {
        [JsonPropertyName("productId")]
        public int ProductId { get; set; }

        [JsonPropertyName("productName")]
        public string ProductName { get; set; } = string.Empty;

        [JsonPropertyName("quantity")]
        [Range(1, 999)]
        public int Quantity { get; set; }

        [JsonPropertyName("price")]
        public decimal Price { get; set; }

        [JsonPropertyName("imageUrl")]
        public string ImageUrl { get; set; } = string.Empty;

        [JsonPropertyName("subtotal")]
        public decimal Subtotal => Quantity * Price;
    }

    public class ShoppingCart
    {
        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("total")]
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
