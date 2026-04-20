namespace Ecommerce.Customer.CartAPI.Models
{
    public class Cart
    {
        public int Id { get; set; }
        public string UserId { get; set; }

        public decimal TotalAmount { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }

        public List<CartItem> Items { get; set; }
    }
}
