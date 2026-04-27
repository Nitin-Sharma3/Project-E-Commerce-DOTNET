using DeliveryService.Models;

namespace DeliveryService.Repositories
{
    public interface IDeliveryRepository
    {
        //Task<IEnumerable<Delivery>> GetAllAsync();
        //Task<IEnumerable<Delivery>> GetByUserIdAsync(int userId);
        //Task<Delivery?> GetByIdAsync(int id);
        //Task<Delivery?> GetByOrderIdAsync(int orderId);
        //Task<Delivery?> GetByTrackingIdAsync(string trackingId);
        //Task<Delivery> CreateAsync(Delivery delivery);
        //Task<Delivery> UpdateAsync(Delivery delivery);
        //Task AddStatusHistoryAsync(DeliveryStatusHistory history);
        //Task SaveChangesAsync();

        Task<IEnumerable<Delivery>> GetAllAsync();

        Task<IEnumerable<Delivery>> GetByUserIdAsync(int userId);

        Task<Delivery?> GetByIdAsync(int id);

        Task<Delivery?> GetByOrderIdAsync(int orderId);

        Task<Delivery?> GetByTrackingIdAsync(string trackingId);

        Task<Delivery> CreateAsync(Delivery delivery);

        Task UpdateAsync(Delivery delivery);   // ✅ FIXED

        Task AddStatusHistoryAsync(DeliveryStatusHistory history);
    }
}