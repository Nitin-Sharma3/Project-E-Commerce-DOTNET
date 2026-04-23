using ECommerceUserApi.DTOs;
using ECommerceUserApi.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ECommerceUserApi.Controllers
{
    [ApiController]
    [Route("api/users")]
    public class UserController : ControllerBase
    {
        private readonly IUserService _service;

        public UserController(IUserService service)
        {
            _service = service;
        }

        [HttpPost("register")]
        public async Task<IActionResult> Register(RegisterDto dto)
        {
            var result = await _service.Register(dto);
            return Ok(result);
        }

        [HttpPost("register-seller")]
        public async Task<IActionResult> RegisterSeller(RegisterDto dto)
        {
            dto.Role = "Seller";
            var result = await _service.Register(dto);
            return Ok(result);
        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginDto dto)
        {
            var token = await _service.Login(dto);
            return Ok(new { token });
        }

        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp(string email, string otp)
        {
            var result = await _service.VerifyOtp(email, otp);
            return Ok(result);
        }

        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok("Logged out successfully");
        }

        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword(string email)
        {
            var token = await _service.ForgotPassword(email);
            return Ok(new { token });
        }

        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword(string token, string newPassword)
        {
            var result = await _service.ResetPassword(token, newPassword);
            return Ok(result);
        }

        [Authorize]
        [HttpGet("profile")]
        public IActionResult Profile()
        {
            return Ok("Protected profile");
        }

        [Authorize(Roles = "Admin")]
        [HttpGet("admin")]
        public IActionResult AdminOnly()
        {
            return Ok("Admin only");
        }

        [Authorize(Roles = "Customer")]
        [HttpGet("customer-dashboard")]
        public IActionResult CustomerDashboard()
        {
            return Ok("Customer Dashboard");
        }

        [Authorize(Roles = "Seller")]
        [HttpGet("seller-dashboard")]
        public IActionResult SellerDashboard()
        {
            return Ok("Seller Dashboard");
        }

        [Authorize(Roles = "Admin,Seller")]
        [HttpGet("manage-products")]
        public IActionResult ManageProducts()
        {
            return Ok("Admin & Seller");
        }
    }
}