using System.ComponentModel.DataAnnotations;
namespace Ecommerce.Customer.WishlistAPI.Models
{
    

    public class WishlistEntity
    {
        public int Id { get; set; }

        [Required]
        [MaxLength(100)]
        public string UserId { get; set; }

        [Required]
        public string ProductId { get; set; }

        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }

        public DateTime CreatedAt { get; set; }
    }
}
