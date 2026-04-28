using AddressAPI.DTOs;
using AddressAPI.Models;
using AddressAPI.Repositories;
using AddressAPI.Services;
namespace AddressAPI.Services
{

    public class AddressService : IAddressService
    {
        private readonly IAddressRepository _addressRepository;

        public AddressService(IAddressRepository addressRepository)
        {
            _addressRepository = addressRepository;
        }

        // ── Mapping ───────────────────────────────────────────

        private static AddressResponseDto MapToDto(Address a) => new()
        {
            Id = a.Id,
            UserId = a.UserId,
            FullName = a.FullName,
            Phone = a.Phone,
            AddressLine1 = a.AddressLine1,
            AddressLine2 = a.AddressLine2,
            City = a.City,
            State = a.State,
            PostalCode = a.PostalCode,
            Country = a.Country,
            IsPrimary = a.IsPrimary,
            CreatedAt = a.CreatedAt,
            UpdatedAt = a.UpdatedAt
        };

      
        // Demotes the current primary (if any) before assigning a new one.
       
        private async Task DemoteCurrentPrimaryAsync(int userId)
        {
            var current = await _addressRepository.GetPrimaryAsync(userId);
            if (current is not null)
            {
                current.IsPrimary = false;
                await _addressRepository.UpdateAsync(current);
            }
        }

        // After the primary is deleted, promotes the address with the lowest Id.
        
        private async Task PromoteNextPrimaryAsync(int userId, int deletedAddressId)
        {
            var next = await _addressRepository.GetNextBySequenceAsync(userId, deletedAddressId);
            if (next is not null)
            {
                next.IsPrimary = true;
                await _addressRepository.UpdateAsync(next);
            }
            
        }

        // ── Public API ────────────────────────────────────────

        public async Task<IEnumerable<AddressResponseDto>> GetAllAsync(int userId)
        {
            var addresses = await _addressRepository.GetAllByUserIdAsync(userId);
            return addresses.Select(MapToDto);
        }

        public async Task<AddressResponseDto> GetByIdAsync(int userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAndUserAsync(addressId, userId)
                          ?? throw new KeyNotFoundException($"Address {addressId} not found.");

            return MapToDto(address);
        }

        public async Task<AddressResponseDto> AddAsync(int userId, CreateAddressDto dto)
        {
            var existingAddresses = await _addressRepository.GetAllByUserIdAsync(userId);
            bool isFirstAddress = !existingAddresses.Any();

            // If it's the first address ever, always make it primary regardless of dto flag
            bool shouldBePrimary = isFirstAddress || dto.IsPrimary;

            if (shouldBePrimary)
                await DemoteCurrentPrimaryAsync(userId);

            var address = new Address
            {
                UserId = userId,
                FullName = dto.FullName,
                Phone = dto.Phone,
                AddressLine1 = dto.AddressLine1,
                AddressLine2 = dto.AddressLine2,
                City = dto.City,
                State = dto.State,
                PostalCode = dto.PostalCode,
                Country = dto.Country,
                IsPrimary = shouldBePrimary
            };

            await _addressRepository.AddAsync(address);
            return MapToDto(address);
        }

        public async Task<AddressResponseDto> UpdateAsync(int userId, int addressId, UpdateAddressDto dto)
        {
            var address = await _addressRepository.GetByIdAndUserAsync(addressId, userId)
                          ?? throw new KeyNotFoundException($"Address {addressId} not found.");

            // Only update fields that were provided (partial update)
            if (dto.FullName is not null) address.FullName = dto.FullName;
            if (dto.Phone is not null) address.Phone = dto.Phone;
            if (dto.AddressLine1 is not null) address.AddressLine1 = dto.AddressLine1;
            if (dto.AddressLine2 is not null) address.AddressLine2 = dto.AddressLine2;
            if (dto.City is not null) address.City = dto.City;
            if (dto.State is not null) address.State = dto.State;
            if (dto.PostalCode is not null) address.PostalCode = dto.PostalCode;
            if (dto.Country is not null) address.Country = dto.Country;

            await _addressRepository.UpdateAsync(address);
            return MapToDto(address);
        }

        public async Task SetPrimaryAsync(int userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAndUserAsync(addressId, userId)
                          ?? throw new KeyNotFoundException($"Address {addressId} not found.");

            if (address.IsPrimary)
                throw new InvalidOperationException("This address is already the primary address.");

            // Demote old primary, then promote the requested one
            await DemoteCurrentPrimaryAsync(userId);

            address.IsPrimary = true;
            await _addressRepository.UpdateAsync(address);
        }

        public async Task DeleteAsync(int userId, int addressId)
        {
            var address = await _addressRepository.GetByIdAndUserAsync(addressId, userId)
                          ?? throw new KeyNotFoundException($"Address {addressId} not found.");

            bool wasPrimary = address.IsPrimary;

            await _addressRepository.DeleteAsync(address);

            // If the deleted address was primary, promote the next one by Id sequence
            if (wasPrimary)
                await PromoteNextPrimaryAsync(userId, addressId);
        }
    }
}
