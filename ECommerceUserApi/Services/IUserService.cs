using ECommerceUserApi.DTOs;

namespace ECommerceUserApi.Services
{
    public interface IUserService
    {
        Task<string> Register(RegisterDto dto);
        Task<string> Login(LoginDto dto);
        Task<string> ForgotPassword(string email);
        Task<string> ResetPassword(string token, string newPassword);
        Task<string> VerifyOtp(string email, string otp);
    }
}