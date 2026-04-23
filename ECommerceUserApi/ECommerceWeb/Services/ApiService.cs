using Newtonsoft.Json;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json.Serialization;

namespace ECommerceWeb.Services
{
    public class ApiService
    {
        private readonly HttpClient _http;

        public ApiService(HttpClient http)
        {
            _http = http;
        }

        public async Task<string> Register(object data)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json");

            var res = await _http.PostAsync("https://localhost:7175/api/users/register", content);
            return await res.Content.ReadAsStringAsync();
        }

        public async Task<string> Login(object data)
        {
            var content = new StringContent(
                JsonConvert.SerializeObject(data),
                Encoding.UTF8,
                "application/json");

            var res = await _http.PostAsync("https://localhost:7175/api/users/login", content);
            return await res.Content.ReadAsStringAsync();
        }
    }
}