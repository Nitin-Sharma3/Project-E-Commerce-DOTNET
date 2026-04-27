using DeliveryService.Data;
using DeliveryService.Models;
using Microsoft.EntityFrameworkCore;

namespace DeliveryService.Repositories
{
    public class DeliveryRepository(DeliveryDbContext db) : IDeliveryRepository
    {
        private IQueryable<Delivery> WithIncludes() =>
            db.Deliveries
                .Include(d => d.Items)
                .Include(d => d.StatusHistory.OrderBy(h => h.Timestamp));

        public async Task<IEnumerable<Delivery>> GetAllAsync() =>
            await WithIncludes().OrderByDescending(d => d.CreatedAt).ToListAsync();

        public async Task<IEnumerable<Delivery>> GetByUserIdAsync(int userId) =>
            await WithIncludes().Where(d => d.UserId == userId)
                .OrderByDescending(d => d.CreatedAt).ToListAsync();

        public async Task<Delivery?> GetByIdAsync(int id) =>
            await WithIncludes().FirstOrDefaultAsync(d => d.Id == id);

        public async Task<Delivery?> GetByOrderIdAsync(int orderId) =>
            await WithIncludes().FirstOrDefaultAsync(d => d.OrderId == orderId);

        public async Task<Delivery?> GetByTrackingIdAsync(string trackingId) =>
            await WithIncludes().FirstOrDefaultAsync(d => d.TrackingId == trackingId);

        public async Task<Delivery> CreateAsync(Delivery delivery)
        {
            db.Deliveries.Add(delivery);
            await db.SaveChangesAsync();
            return delivery;
        }

        //public async Task<Delivery> UpdateAsync(Delivery delivery)
        //{
        //    db.Deliveries.Update(delivery);
        //    await db.SaveChangesAsync();
        //    return delivery;
        //}
        public async Task UpdateAsync(Delivery delivery)
        {
            db.Deliveries.Update(delivery);
            await db.SaveChangesAsync();
        }
        public async Task AddStatusHistoryAsync(DeliveryStatusHistory history)
        {
            db.StatusHistories.Add(history);
            await db.SaveChangesAsync();
        }

        public async Task SaveChangesAsync() => await db.SaveChangesAsync();
    }
}
