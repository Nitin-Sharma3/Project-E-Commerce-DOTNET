using Newtonsoft.Json;
using System.Text;

namespace JAVLN.Web.Services
{
    public class UserApiService
    {
        private readonly HttpClient _http;
        private readonly string _base;

        public UserApiService(IHttpClientFactory factory, IConfiguration config)
        {
            _http = factory.CreateClient();
            _base = config["UserApi:BaseUrl"]!;
        }

        // ───────────────── REGISTER ─────────────────
        public async Task<(bool success, string message)> Register(object dto)
        {
            return await Post("/register", dto);
        }

        // ───────────────── REGISTER SELLER ──────────
        public async Task<(bool success, string message)> RegisterSeller(object dto)
        {
            return await Post("/register-seller", dto);
        }

        // ───────────────── LOGIN ────────────────────
        public async Task<(bool success, string token)> Login(object dto)
        {
            var (success, body) = await Post("/login", dto);
            return (success, body);
        }

        // ───────────────── VERIFY OTP ───────────────
        public async Task<(bool success, string message)> VerifyOtp(string email, string otp)
        {
            return await Post("/verify-otp", new
            {
                Email = email,
                Otp = otp
            });
        }

        // ───────────────── FORGOT PASSWORD ──────────
        public async Task<(bool success, string message)> ForgotPassword(string email)
        {
            return await Post("/forgot-password", new
            {
                Email = email
            });
        }

        // ───────────────── RESET PASSWORD ───────────
        public async Task<(bool success, string message)> ResetPassword(string token, string newPassword)
        {
            return await Post("/reset-password", new
            {
                Token = token,
                NewPassword = newPassword
            });
        }

        // ───────────────── COMMON POST METHOD ───────
        private async Task<(bool success, string body)> Post(string path, object dto)
        {
            var json = JsonConvert.SerializeObject(dto);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _http.PostAsync(_base + path, content);
            var body = await response.Content.ReadAsStringAsync();

            return (response.IsSuccessStatusCode, body);
        }
    }
}