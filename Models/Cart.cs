using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Text.Json.Serialization;

namespace EStore.Models
{
    public class CartItem
    {
        [Key]
        public int Id { get; set; }

        [ForeignKey(nameof(ShoppingCart))]
        public int CartId { get; set; }
        public ShoppingCart? ShoppingCart { get; set; }

        [JsonPropertyName("productId")]
        public int ProductId { get; set; }
        public Product? Product { get; set; }

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
        [Key]
        public int Id { get; set; }

        [JsonPropertyName("userId")]
        public string UserId { get; set; } = string.Empty;

        [JsonPropertyName("items")]
        public List<CartItem> Items { get; set; } = new List<CartItem>();

        [JsonPropertyName("createdAt")]
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("updatedAt")]
        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        [JsonPropertyName("total")]
        public decimal Total => Items.Sum(i => i.Subtotal);
    }
}
