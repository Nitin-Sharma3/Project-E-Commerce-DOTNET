using CustomerDashboard.DTOs;

namespace CustomerDashboard.Services
{
    using System.Net.Http.Json;

    public class AddressService
    {
        private readonly HttpClient _httpClient;

        public AddressService(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        // 🟢 GET ALL
        public async Task<List<AddressDto>> GetAddresses()
        {
            return await _httpClient.GetFromJsonAsync<List<AddressDto>>("api/address");
        }

        // 🟢 GET BY ID
        public async Task<AddressDto> GetAddress(int id)
        {
            return await _httpClient.GetFromJsonAsync<AddressDto>($"api/address/{id}");
        }

        // 🟢 ADD
        public async Task AddAddress(CreateAddressDto dto)
        {
            await _httpClient.PostAsJsonAsync("api/address", dto);
        }

        // 🟡 UPDATE
        public async Task UpdateAddress(int id, UpdateAddressDto dto)
        {
            await _httpClient.PutAsJsonAsync($"api/address/{id}", dto);
        }

        // 🔴 DELETE
        public async Task DeleteAddress(int id)
        {
            await _httpClient.DeleteAsync($"api/address/{id}");
        }
    }
}
