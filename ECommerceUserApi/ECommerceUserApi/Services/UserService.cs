using ECommerceUserApi.DTOs;
using ECommerceUserApi.Models;
using ECommerceUserApi.Repositories;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace ECommerceUserApi.Services
{
    public class UserService : IUserService
    {
        private readonly IUserRepository _repo;
        private readonly IConfiguration _config;

        public UserService(IUserRepository repo, IConfiguration config)
        {
            _repo = repo;
            _config = config;
        }

        public async Task<string> Register(RegisterDto dto)
        {
            var existing = await _repo.GetByEmail(dto.Email);
            if (existing != null)
                throw new Exception("User already exists");

            UserRole role = UserRole.Customer;

            if (!string.IsNullOrEmpty(dto.Role))
            {
                if (dto.Role.Equals("Admin", StringComparison.OrdinalIgnoreCase))
                    role = UserRole.Admin;
                else if (dto.Role.Equals("Seller", StringComparison.OrdinalIgnoreCase))
                    role = UserRole.Seller;
            }

            var otp = new Random().Next(100000, 999999).ToString();

            var user = new User
            {
                Name = dto.Name,
                Email = dto.Email,
                Password = BCrypt.Net.BCrypt.HashPassword(dto.Password),
                Phone = dto.Phone,
                Role = role,
                Otp = otp,
                OtpExpiry = DateTime.Now.AddMinutes(5),
                IsVerified = false
            };

            await _repo.Create(user);

            return $"User Registered. OTP: {otp}";
        }

        public async Task<string> Login(LoginDto dto)
        {
            var user = await _repo.GetByEmail(dto.Email);

            if (user == null || !BCrypt.Net.BCrypt.Verify(dto.Password, user.Password))
                throw new Exception("Invalid credentials");

            if (!user.IsVerified)
                throw new Exception("Please verify OTP first");

            return GenerateJwt(user);
        }

        private string GenerateJwt(User user)
        {
            var key = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(_config["Jwt:Key"]!)
            );

            var creds = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);

            var claims = new[]
            {
                new Claim("id", user.Id.ToString()),
                new Claim(ClaimTypes.Email, user.Email),
                new Claim(ClaimTypes.Role, user.Role.ToString())
            };

            var token = new JwtSecurityToken(
                claims: claims,
                expires: DateTime.Now.AddHours(1),
                signingCredentials: creds
            );

            return new JwtSecurityTokenHandler().WriteToken(token);
        }

        public async Task<string> ForgotPassword(string email)
        {
            var user = await _repo.GetByEmail(email);
            if (user == null)
                throw new Exception("User not found");

            user.ResetToken = Guid.NewGuid().ToString();
            user.ResetTokenExpiry = DateTime.Now.AddMinutes(15);

            await _repo.Update(user);

            return user.ResetToken!;
        }

        public async Task<string> ResetPassword(string token, string newPassword)
        {
            var user = await _repo.GetByResetToken(token);

            if (user == null || user.ResetTokenExpiry < DateTime.Now)
                throw new Exception("Invalid or expired token");

            user.Password = BCrypt.Net.BCrypt.HashPassword(newPassword);
            user.ResetToken = null;
            user.ResetTokenExpiry = null;

            await _repo.Update(user);

            return "Password Reset Successful";
        }

        public async Task<string> VerifyOtp(string email, string otp)
        {
            var user = await _repo.GetByEmail(email);

            if (user == null)
                throw new Exception("User not found");

            if (user.Otp != otp || user.OtpExpiry < DateTime.Now)
                throw new Exception("Invalid or expired OTP");

            user.IsVerified = true;
            user.Otp = null;
            user.OtpExpiry = null;

            await _repo.Update(user);

            return "OTP Verified Successfully";
        }
    }
}