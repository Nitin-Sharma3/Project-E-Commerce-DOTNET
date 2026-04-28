using System.ComponentModel.DataAnnotations;

namespace ECommerceUserApi.Models
{
    public enum UserRole
    {
        Admin,
        Customer,
        Seller
    }

    public class User
    {
        public int Id { get; set; }

        [Required]
        public string Name { get; set; } = null!;

        [Required]
        public string Email { get; set; } = null!;

        public string Password { get; set; } = null!;
        public string Phone { get; set; } = null!;

        public UserRole Role { get; set; } = UserRole.Customer;

        public string? ResetToken { get; set; }
        public DateTime? ResetTokenExpiry { get; set; }

        public string? Otp { get; set; }
        public DateTime? OtpExpiry { get; set; }
        public bool IsVerified { get; set; } = false;
    }
}