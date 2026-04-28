using DeliveryService.DTOs;
using DeliveryService.Models;

namespace DeliveryService.Services
{
    public interface IDeliveryService
    {
        Task<IEnumerable<DeliveryResponseDto>> GetAllAsync();
        Task<DeliveryResponseDto?> GetByIdAsync(int id);
        Task<IEnumerable<DeliveryResponseDto>> GetByUserIdAsync(int userId);
        Task<DeliveryResponseDto?> GetByOrderIdAsync(int orderId);
        Task<TrackingResponseDto?> GetTrackingAsync(string trackingId);
        Task<IEnumerable<DeliveryMapPointDto>> GetAllForMapAsync();
        Task<DeliveryResponseDto> CreateAsync(CreateDeliveryDto dto);
        Task<DeliveryResponseDto> CreateFromOrderAsync(int orderId, int userId);
        Task<DeliveryResponseDto> UpdateStatusAsync(UpdateDeliveryStatusDto dto);
        Task<DeliveryResponseDto> MarkDeliveredAsync(MarkDeliveredDto dto);
        Task<DeliveryResponseDto> AssignAgentAsync(AssignAgentDto dto);
    }
}