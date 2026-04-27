using AddressAPI.Models;

namespace AddressAPI.Repositories
{
    public interface IAddressRepository
    {
        Task<IEnumerable<Address>> GetAllByUserIdAsync(int userId);
        Task<Address?> GetByIdAndUserAsync(int addressId, int userId);
        Task<Address?> GetPrimaryAsync(int userId);

        /// <summary>
        /// Returns the address with the lowest Id for the user,
        /// excluding the given addressId. Used for fallback promotion.
        /// </summary>
        Task<Address?> GetNextBySequenceAsync(int userId, int excludeAddressId);

        Task AddAsync(Address address);
        Task UpdateAsync(Address address);
        Task DeleteAsync(Address address);
        Task SaveChangesAsync();
    }
}
