namespace DeliveryService.Models
{
    public class DeliveryStatusHistory
    {
        public int Id { get; set; }
        public int DeliveryId { get; set; }
        public Delivery Delivery { get; set; } = null!;

        public DeliveryStatus Status { get; set; }
        public string? Remarks { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime Timestamp { get; set; } = DateTime.UtcNow;
        public string UpdatedBy { get; set; } = "System";
    }
}
