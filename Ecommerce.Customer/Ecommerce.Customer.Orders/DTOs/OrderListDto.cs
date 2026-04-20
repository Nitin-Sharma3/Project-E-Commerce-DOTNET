namespace Ecommerce.Customer.OrderAPI.DTOs
{
    public class OrderListDto
    {
        public int Id { get; set; }
        public string OrderNumber { get; set; }
        public decimal TotalAmount { get; set; }
        public int ItemCount { get; set; }
        public string Status { get; set; }
        public string PaymentStatus { get; set; }
        public DateTime PlacedAt { get; set; }
    }
}