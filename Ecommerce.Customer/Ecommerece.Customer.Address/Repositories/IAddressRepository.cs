using Ecommerece.Customer.Address.Models;

namespace Ecommerece.Customer.Address.Repositories
{
    public interface IAddressRepository
    {
        Task<List<AddressEntity>> GetByUserIdAsync(string userId);
        Task<AddressEntity> GetByIdAsync(int id);
        Task AddAsync(AddressEntity address);
        Task UpdateAsync(AddressEntity address);
        Task DeleteAsync(AddressEntity address);
        Task ResetPrimaryAsync(string userId);
    }
}
