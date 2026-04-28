using Microsoft.EntityFrameworkCore;
using AddressAPI.Data;
using AddressAPI.Models;
using AddressAPI.Repositories;
namespace AddressAPI.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly AppDbContext _context;

        public AddressRepository(AppDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Address>> GetAllByUserIdAsync(int userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .OrderByDescending(a => a.IsPrimary)   // primary first
                .ThenBy(a => a.Id)
                .ToListAsync();
        }

        public async Task<Address?> GetByIdAndUserAsync(int addressId, int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.Id == addressId && a.UserId == userId);
        }

        public async Task<Address?> GetPrimaryAsync(int userId)
        {
            return await _context.Addresses
                .FirstOrDefaultAsync(a => a.UserId == userId && a.IsPrimary);
        }

        public async Task<Address?> GetNextBySequenceAsync(int userId, int excludeAddressId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId && a.Id != excludeAddressId)
                .OrderBy(a => a.Id)   // lowest Id wins
                .FirstOrDefaultAsync();
        }

        public async Task AddAsync(Address address)
        {
            _context.Addresses.Add(address);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Address address)
        {
            address.UpdatedAt = DateTime.UtcNow;
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(Address address)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }

        public async Task SaveChangesAsync()
        {
            await _context.SaveChangesAsync();
        }
    }
}
