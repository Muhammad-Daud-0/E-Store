using Microsoft.AspNetCore.Identity;

namespace EStore.Models
{
    // Extend Identity user with application-specific profile fields
    public class ApplicationUser : IdentityUser
    {
        public string? FullName { get; set; } = string.Empty;
        public string? ShippingAddress { get; set; } = string.Empty;
        public string? City { get; set; } = string.Empty;
    }
}
