namespace DeliveryMVC.Models
{
    public enum DeliveryStatus { Pending, Packed, Shipped, OutForDelivery, Delivered, Failed }

    public class DeliveryViewModel
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public int UserId { get; set; }
        public string RecipientName { get; set; } = "";
        public string FullAddress { get; set; } = "";
        public string ContactPhone { get; set; } = "";
        public string TrackingId { get; set; } = "";
        public string Status { get; set; } = "";
        public string? DeliveryAgentName { get; set; }
        public string? DeliveryAgentPhone { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime EstimatedDeliveryDate { get; set; }
        public DateTime? ActualDeliveryDate { get; set; }
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public List<DeliveryItemViewModel> Items { get; set; } = [];
        public List<StatusHistoryViewModel> History { get; set; } = [];
    }

    public class DeliveryItemViewModel
    {
        public string ProductName { get; set; } = "";
        public int Quantity { get; set; }
        public decimal UnitPrice { get; set; }
        public decimal LineTotal { get; set; }
    }

    public class StatusHistoryViewModel
    {
        public string Status { get; set; } = "";
        public string? Remarks { get; set; }
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string UpdatedBy { get; set; } = "";
    }

    public class TrackingViewModel
    {
        public string TrackingId { get; set; } = "";
        public string RecipientName { get; set; } = "";
        public string DestinationAddress { get; set; } = "";
        public string CurrentStatus { get; set; } = "";
        public double? CurrentLatitude { get; set; }
        public double? CurrentLongitude { get; set; }
        public DateTime EstimatedDelivery { get; set; }
        public DateTime? ActualDelivery { get; set; }
        public string? AgentName { get; set; }
        public string? AgentPhone { get; set; }
        public List<TrackingPointViewModel> Timeline { get; set; } = [];
    }

    public class TrackingPointViewModel
    {
        public string Status { get; set; } = "";
        public string? Location { get; set; }
        public double? Latitude { get; set; }
        public double? Longitude { get; set; }
        public DateTime Timestamp { get; set; }
        public string? Remarks { get; set; }
        public bool IsCurrent { get; set; }
    }

    public class ApiResponse<T>
    {
        public bool Success { get; set; }
        public string Message { get; set; } = "";
        public T? Data { get; set; }
    }
}
