using DeliveryService.Models;

namespace DeliveryService.DTOs
{
    // ── Request DTOs ──────────────────────────────────────────────────────────────

    public record CreateDeliveryDto(
        int OrderId,
        int UserId,
        string RecipientName,
        string AddressLine1,
        string? AddressLine2,
        string City,
        string State,
        string Pincode,
        string ContactPhone,
        string? ContactEmail,
        List<DeliveryItemDto> Items
    );

    public record DeliveryItemDto(
        int ProductId,
        string ProductName,
        int Quantity,
        decimal UnitPrice,
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

    public record MarkDeliveredDto(int DeliveryId, string UpdatedBy = "Admin");

    // ── Response DTOs ─────────────────────────────────────────────────────────────

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

    public record ApiResponse<T>(bool Success, string Message, T? Data, string? Error = null);
}
