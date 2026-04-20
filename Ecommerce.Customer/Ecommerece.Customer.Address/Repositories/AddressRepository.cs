using Ecommerece.Customer.Address.Data;
using Ecommerece.Customer.Address.Models;
using Microsoft.EntityFrameworkCore;

namespace Ecommerece.Customer.Address.Repositories
{
    public class AddressRepository : IAddressRepository
    {
        private readonly EcommereceCustomerAddressContext _context;

        public AddressRepository(EcommereceCustomerAddressContext context)
        {
            _context = context;
        }

        public async Task<List<AddressEntity>> GetByUserIdAsync(string userId)
        {
            return await _context.Addresses
                .Where(a => a.UserId == userId)
                .ToListAsync();
        }

        public async Task<AddressEntity> GetByIdAsync(int id)
        {
            return await _context.Addresses.FindAsync(id);
        }

        public async Task AddAsync(AddressEntity address)
        {
            await _context.Addresses.AddAsync(address);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(AddressEntity address)
        {
            _context.Addresses.Update(address);
            await _context.SaveChangesAsync();
        }

        public async Task DeleteAsync(AddressEntity address)
        {
            _context.Addresses.Remove(address);
            await _context.SaveChangesAsync();
        }

        public async Task ResetPrimaryAsync(string userId)
        {
            await _context.Addresses
                .Where(a => a.UserId == userId && a.IsPrimary)
                .ExecuteUpdateAsync(a => a.SetProperty(x => x.IsPrimary, false));
        }
    }
}
