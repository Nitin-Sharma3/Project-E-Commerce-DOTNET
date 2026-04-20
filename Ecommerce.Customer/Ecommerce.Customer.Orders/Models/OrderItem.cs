namespace Ecommerce.Customer.OrderAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        // Snapshot of product at time of order (never rely on live product data)
        public string ProductId { get; set; }
        public string Name { get; set; }
        public string Image { get; set; }
        public decimal UnitPrice { get; set; }
        public int Quantity { get; set; }
        public decimal SubTotal { get; set; }    // UnitPrice * Quantity
    }
}