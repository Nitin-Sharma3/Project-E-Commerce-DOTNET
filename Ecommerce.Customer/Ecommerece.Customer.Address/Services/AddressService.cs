using Ecommerece.Customer.Address.DTOs;
using Ecommerece.Customer.Address.Models;
using Ecommerece.Customer.Address.Repositories;

namespace Ecommerece.Customer.Address.Services
{
    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _repo;

        public AddressService(IAddressRepository repo)
        {
            _repo = repo;
        }

        public async Task<List<AddressResponseDto>> GetAddresses(string userId)
        {
            var addresses = await _repo.GetByUserIdAsync(userId);

            return addresses.Select(MapToDto).ToList();
        }

        public async Task<AddressResponseDto> GetAddressById(string userId, int id)
        {
            var address = await _repo.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
                return null;

            return MapToDto(address);
        }

        public async Task<AddressResponseDto> AddAddress(string userId, CreateAddressDto dto)
        {
            if (dto.IsPrimary)
            {
                await _repo.ResetPrimaryAsync(userId);
            }

            var entity = new AddressEntity
            {
                UserId = userId,
                FullName = dto.FullName,
                PhoneNumber = dto.PhoneNumber,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                Type = (AddressType)dto.Type,
                IsPrimary = dto.IsPrimary,
                Label = dto.Label
            };

            await _repo.AddAsync(entity);

            return MapToDto(entity);
        }

        public async Task<AddressResponseDto> UpdateAddress(string userId, int id, UpdateAddressDto dto)
        {
            var address = await _repo.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
                return null;

            if (dto.IsPrimary)
            {
                await _repo.ResetPrimaryAsync(userId);
                address.IsPrimary = true;
            }
            else
            {
                address.IsPrimary = false;
            }

            address.FullName = dto.FullName;
            address.PhoneNumber = dto.PhoneNumber;
            address.AddressLine1 = dto.AddressLine1;
            address.AddressLine2 = dto.AddressLine2;
            address.City = dto.City;
            address.State = dto.State;
            address.PostalCode = dto.PostalCode;
            address.Country = dto.Country;
            address.Type = dto.Type;
            address.IsPrimary = dto.IsPrimary;
            address.Label = dto.Label;

            await _repo.UpdateAsync(address);

            return MapToDto(address);
        }

        public async Task<bool> DeleteAddress(string userId, int id)
        {
            var address = await _repo.GetByIdAsync(id);

            if (address == null || address.UserId != userId)
                return false;

            await _repo.DeleteAsync(address);
            return true;
        }

        private AddressResponseDto MapToDto(AddressEntity a)
        {
            return new AddressResponseDto
            {
                Id = a.Id,
                FullName = a.FullName,
                PhoneNumber = a.PhoneNumber,
                AddressLine1 = a.AddressLine1,
                AddressLine2 = a.AddressLine2,
                City = a.City,
                State = a.State,
                PostalCode = a.PostalCode,
                Country = a.Country,
                Type = a.Type.ToString(),
                IsPrimary = a.IsPrimary,
                Label = a.Label
            };
        }
    }
}
