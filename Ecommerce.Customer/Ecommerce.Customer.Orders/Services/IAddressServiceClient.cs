using Ecommerece.Customer.Address.DTOs;

namespace Ecommerce.Customer.OrderAPI.Services
{
    public interface IAddressServiceClient
    {
        Task<AddressResponseDto?> GetAddress(string userId, int addressId);
    }
}