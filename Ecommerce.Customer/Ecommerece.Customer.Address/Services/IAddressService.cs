using Ecommerece.Customer.Address.DTOs;

namespace Ecommerece.Customer.Address.Services
{
    public interface IAddressService
    {
        Task<List<AddressResponseDto>> GetAddresses(string userId);
        Task<AddressResponseDto> GetAddressById(string userId, int id);
        Task<AddressResponseDto> AddAddress(string userId, CreateAddressDto dto);
        Task<AddressResponseDto> UpdateAddress(string userId, int id, UpdateAddressDto dto);
        Task<bool> DeleteAddress(string userId, int id);
    }
}



