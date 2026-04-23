
    using OrderAPI.DTOs;
    using OrderAPI.HttpClients;
    using System.Net.Http.Json;
namespace OrderAPI.HttpClients
{
   
    public class AddressClient : IAddressClient
    {
        private readonly HttpClient _httpClient;

        public AddressClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AddressDto?> GetAddressAsync(int userId, int addressId)
        {
            var response = await _httpClient.GetAsync($"api/users/{userId}/addresses/{addressId}");

            if (!response.IsSuccessStatusCode) return null;

            return await response.Content.ReadFromJsonAsync<AddressDto>();
        }
    }
}
