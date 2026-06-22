namespace EStore.Models
{
    public class CartItemAddRequest
    {
        public int ProductId { get; set; }
        public int Quantity { get; set; } = 1;
    }
}
