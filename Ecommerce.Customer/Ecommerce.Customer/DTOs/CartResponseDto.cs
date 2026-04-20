namespace Ecommerce.Customer.CartAPI.DTOs
{
    public class CartResponseDto
    {
        public string UserId { get; set; }
        public List<CartItemResponseDto> Items { get; set; }
        public decimal TotalAmount { get; set; }
    }
}
