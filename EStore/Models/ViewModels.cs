using System.ComponentModel.DataAnnotations;
using Microsoft.AspNetCore.Identity;

namespace EStore.Models
{
    public class UserDashboardViewModel
    {
        public IdentityUser? User { get; set; }
        public List<Order> Orders { get; set; } = new List<Order>();
    }

    public class AdminDashboardViewModel
    {
        public List<Category> Categories { get; set; } = new List<Category>();
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Order> Orders { get; set; } = new List<Order>();
        public List<IdentityUser> Users { get; set; } = new List<IdentityUser>();
        public Dictionary<string, string> UserRoles { get; set; } = new Dictionary<string, string>();
        public IdentityUser? CurrentUser { get; set; }
    }

    public class HomePageViewModel
    {
        public List<Product> Products { get; set; } = new List<Product>();
        public List<Category> Categories { get; set; } = new List<Category>();
        public int? SelectedCategoryId { get; set; }
        public string? SearchTerm { get; set; }

        // Filters
        public decimal? MinPrice { get; set; }
        public decimal? MaxPrice { get; set; }
        public decimal? MinRating { get; set; }
        public decimal MaxPriceInDb { get; set; }

        public string ViewMode { get; set; } = "pages"; // "pages" or "scroll"

        // Pagination
        public int CurrentPage { get; set; } = 1;
        public int PageSize { get; set; } = 12;
        public int TotalCount { get; set; }
        public int TotalPages => (TotalCount + PageSize - 1) / PageSize;
    }

    public class CheckoutViewModel
    {
        public ShoppingCart? Cart { get; set; }
        public string? CustomerEmail { get; set; }

        [Required(ErrorMessage = "Full name is required")]
        [MaxLength(100)]
        public string CustomerName { get; set; } = string.Empty;

        [Required(ErrorMessage = "Shipping address is required")]
        [MaxLength(300)]
        public string ShippingAddress { get; set; } = string.Empty;

        [Required(ErrorMessage = "City is required")]
        [MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required(ErrorMessage = "Phone number is required")]
        [Phone(ErrorMessage = "Invalid phone number")]
        public string PhoneNumber { get; set; } = string.Empty;
    }

    public class OrderConfirmationViewModel
    {
        public int OrderId { get; set; }
        public decimal TotalPrice { get; set; }
        public DateTime OrderDate { get; set; }
        public string CustomerName { get; set; } = string.Empty;
        public string ShippingAddress { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public List<OrderItem> Items { get; set; } = new List<OrderItem>();
    }
}
