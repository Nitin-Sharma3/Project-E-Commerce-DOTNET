using DeliveryService.DTOs;

namespace DeliveryService.Services
{
    public interface IDeliveryService
    {
        Task<IEnumerable<DeliveryResponseDto>> GetAllAsync();
        Task<DeliveryResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<DeliveryResponseDto>> GetByUserIdAsync(int userId);
        Task<TrackingResponseDto?> GetTrackingAsync(string trackingId);
        Task<DeliveryResponseDto> CreateAsync(CreateDeliveryDto dto);
        Task<DeliveryResponseDto> UpdateStatusAsync(UpdateDeliveryStatusDto dto);
        Task<DeliveryResponseDto> MarkDeliveredAsync(MarkDeliveredDto dto);
        Task<IEnumerable<DeliveryMapPointDto>> GetAllForMapAsync();
    }
}
