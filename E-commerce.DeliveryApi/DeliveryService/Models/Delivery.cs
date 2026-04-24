using System.ComponentModel.DataAnnotations;

namespace DeliveryService.Models
{
    public class Delivery
    {
        public int Id { get; set; }

        [Required] public int OrderId { get; set; }
        [Required] public int UserId { get; set; }

        // Address
        [Required] public string RecipientName { get; set; } = string.Empty;
        [Required] public string AddressLine1 { get; set; } = string.Empty;
        public string? AddressLine2 { get; set; }
        [Required] public string City { get; set; } = string.Empty;
        [Required] public string State { get; set; } = string.Empty;
        [Required] public string Pincode { get; set; } = string.Empty;
        [Required] public string ContactPhone { get; set; } = string.Empty;
        public string? ContactEmail { get; set; }

        // Tracking
        [Required] public string TrackingId { get; set; } = string.Empty;
        public DeliveryStatus Status { get; set; } = DeliveryStatus.Pending;

        // Dates
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }

        // GPS (simulated)
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }

        // Delivery partner
        public string? DeliveryAgentName { get; set; }
        public string? DeliveryAgentPhone { get; set; }

        // Relations
        public ICollection<DeliveryItem> Items { get; set; } = [];
        public ICollection<DeliveryStatusHistory> StatusHistory { get; set; } = [];
    }

    public enum DeliveryStatus
    {
        Pending = 0,
        Packed = 1,
        Shipped = 2,
        OutForDelivery = 3,
        Delivered = 4,
        Failed = 5
    }
}
