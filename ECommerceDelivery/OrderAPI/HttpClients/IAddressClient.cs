
    using OrderAPI.DTOs;
namespace OrderAPI.HttpClients
{

    
    public interface IAddressClient
    {
        /// <summary>
        /// Fetches a specific address that belongs to the given user.
        /// Returns null if not found or if the address does not belong to the user.
        /// </summary>
        Task<AddressDto?> GetAddressAsync(int userId, int addressId);
    }
}
