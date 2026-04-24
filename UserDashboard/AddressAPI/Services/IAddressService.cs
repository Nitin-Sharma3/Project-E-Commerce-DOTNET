using AddressAPI.DTOs;

namespace AddressAPI.Services
{
    public interface IAddressService
    {
        Task<IEnumerable<AddressResponseDto>> GetAllAsync(int userId);
        Task<AddressResponseDto> GetByIdAsync(int userId, int addressId);
        Task<AddressResponseDto> AddAsync(int userId, CreateAddressDto dto);
        Task<AddressResponseDto> UpdateAsync(int userId, int addressId, UpdateAddressDto dto);
        Task SetPrimaryAsync(int userId, int addressId);
        Task DeleteAsync(int userId, int addressId);
    }
}
