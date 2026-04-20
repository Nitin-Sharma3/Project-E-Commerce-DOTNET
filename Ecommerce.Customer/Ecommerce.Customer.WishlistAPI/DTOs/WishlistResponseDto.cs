namespace Ecommerce.Customer.WishlistAPI.DTOs
{
    public class WishlistResponseDto
    {
        public int Id { get; set; }

        public string ProductId { get; set; }

        public string? ProductName { get; set; }
        public decimal? Price { get; set; }
        public string? ImageUrl { get; set; }
    }
}
