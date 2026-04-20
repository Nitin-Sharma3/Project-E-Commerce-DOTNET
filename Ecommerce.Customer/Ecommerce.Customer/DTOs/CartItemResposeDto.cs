namespace Ecommerce.Customer.CartAPI.DTOs
{
    public class CartItemResponseDto
    {
        public string ProductId { get; set; }
        public string Name { get; set; }
        public decimal Price { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }
        public string Image { get; set; }
    }
}
