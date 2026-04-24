namespace DeliveryService.Models
{
    public class DeliveryItem
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public Delivery Delivery { get; set; } = null!;

        public int ProductId { get; set; }
        public string ProductName { get; set; } = string.Empty;
        public string? ImageUrl { get; set; }
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
    }
}
