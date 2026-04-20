using System;
using System.Net.Http;
using System.Net.Http.Json;
using System.Threading.Tasks;
using Ecommerece.Customer.Address.DTOs;

namespace Ecommerce.Customer.OrderAPI.Services
{
    public class AddressServiceClient : IAddressServiceClient
    {
        private readonly HttpClient _httpClient;

        public AddressServiceClient(HttpClient httpClient)
        {
            _httpClient = httpClient;
        }

        public async Task<AddressResponseDto?> GetAddress(string userId, int addressId)
        {
            // GET /api/address/{addressId}?userId={userId}
            var url = $"/api/address/{addressId}?userId={Uri.EscapeDataString(userId)}";
            var response = await _httpClient.GetAsync(url);
            if (!response.IsSuccessStatusCode)
                return null;

            return await response.Content.ReadFromJsonAsync<AddressResponseDto?>();
        }
    }
}
