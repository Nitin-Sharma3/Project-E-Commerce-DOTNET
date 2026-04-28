namespace OrderAPI.Models
{
    public class OrderItem
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;  // snapshot at order time
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }                   // snapshot at order time
        public decimal Subtotal => Quantity * UnitPrice;

        public Order Order { get; set; } = null!;
    }
}
