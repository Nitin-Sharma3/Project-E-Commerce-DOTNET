using DeliveryService.Models;

namespace DeliveryService.DTOs;

// ── Inbound ───────────────────────────────────────────────────────────────

public record CreateDeliveryDto(
    int OrderId,
    int UserId,
    string RecipientName,
    string AddressLine1,
    string? AddressLine2,
    string City,
    string State,
    string Pincode,
    string Country,
    string ContactPhone,
    string? ContactEmail,
    List<DeliveryItemDto> Items
);

public record DeliveryItemDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    string? ImageUrl = null
);

public record UpdateDeliveryStatusDto(
    int DeliveryId,
    DeliveryStatus NewStatus,
    string? Remarks = null,
    string? Location = null,
    double? Latitude = null,
    double? Longitude = null,
    string UpdatedBy = "Admin"
);

public record MarkDeliveredDto(
    int DeliveryId,
    string UpdatedBy = "Admin"
);

public record AssignAgentDto(
    int DeliveryId,
    string AgentName,
    string AgentPhone
);

// ── Outbound ──────────────────────────────────────────────────────────────

public record DeliveryResponseDto(
    int Id,
    int OrderId,
    int UserId,
    string RecipientName,
    string FullAddress,
    string ContactPhone,
    string? ContactEmail,
    string TrackingId,
    string Status,
    string? DeliveryAgentName,
    string? DeliveryAgentPhone,
    DateTime CreatedAt,
    DateTime EstimatedDeliveryDate,
    DateTime? ActualDeliveryDate,
    double? CurrentLatitude,
    double? CurrentLongitude,
    List<DeliveryItemResponseDto> Items,
    List<StatusHistoryDto> History
);

public record DeliveryItemResponseDto(
    int ProductId,
    string ProductName,
    int Quantity,
    decimal UnitPrice,
    decimal Subtotal,
    decimal LineTotal,
    string? ImageUrl
);

public record StatusHistoryDto(
    string Status,
    string? Remarks,
    string? Location,
    double? Latitude,
    double? Longitude,
    DateTime Timestamp,
    string UpdatedBy
);

public record TrackingResponseDto(
    string TrackingId,
    string RecipientName,
    string DestinationAddress,
    string CurrentStatus,
    double? CurrentLatitude,
    double? CurrentLongitude,
    DateTime EstimatedDelivery,
    DateTime? ActualDelivery,
    string? AgentName,
    string? AgentPhone,
    List<TrackingPointDto> Timeline
);

public record TrackingPointDto(
    string Status,
    string? Location,
    double? Latitude,
    double? Longitude,
    DateTime Timestamp,
    string? Remarks,
    bool IsCurrent
);

public record DeliveryMapPointDto(
    int DeliveryId,
    int OrderId,
    string TrackingId,
    string RecipientName,
    string FullAddress,
    string Status,
    double Latitude,
    double Longitude,
    DateTime EstimatedDelivery,
    bool IsOverdue,
    string? AgentName,
    List<RoutePointDto> RouteHistory
);

public record RoutePointDto(
    string Status,
    double Latitude,
    double Longitude,
    DateTime Timestamp,
    string? Location
);

// ── OrderAPI mirrors — matches your actual OrderAPI exactly ───────────────

public class OrderApiResponse
{
    public int Id { get; set; }
    public int UserId { get; set; }           // int — confirmed
    public DateTime OrderDate { get; set; }
    public string Status { get; set; } = "";
    public decimal TotalAmount { get; set; }
    public OrderApiShippingAddress ShippingAddress { get; set; } = new();
    public List<OrderApiItem> Items { get; set; } = [];
}

public class OrderApiShippingAddress
{
    public string FullName { get; set; } = "";      // → RecipientName
    public string Phone { get; set; } = "";         // → ContactPhone
    public string AddressLine1 { get; set; } = "";
    public string? AddressLine2 { get; set; }
    public string City { get; set; } = "";
    public string State { get; set; } = "";
    public string PostalCode { get; set; } = "";    // → Pincode
    public string Country { get; set; } = "";
}

public class OrderApiItem
{
    public int Id { get; set; }
    public int ProductId { get; set; }              // int — confirmed
    public string ProductName { get; set; } = "";
    public int Quantity { get; set; }
    public decimal UnitPrice { get; set; }
    public decimal Subtotal { get; set; }
}

// OrderAPI status update — matches PATCH api/orders/{id}/status
public class OrderStatusUpdatePayload
{
    public string Status { get; set; } = "";
}

// ProductAPI mirror — matches GET api/product/{id}
public class ProductApiResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = "";
    public decimal Price { get; set; }
    public string? ImageUrl1 { get; set; }
    public string Category { get; set; } = "";
}

public record ApiResponse<T>(
    bool Success,
    string Message,
    T? Data,
    string? Error = null
);