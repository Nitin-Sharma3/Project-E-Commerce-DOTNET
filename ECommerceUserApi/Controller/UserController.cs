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

        // ✅ REGISTER
        [HttpPost("register")]
        public async Task<IActionResult> Register([FromBody] RegisterDto dto)
        {
            try
            {
                var result = await _service.Register(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ REGISTER SELLER
        [HttpPost("register-seller")]
        public async Task<IActionResult> RegisterSeller([FromBody] RegisterDto dto)
        {
            try
            {
                dto.Role = "Seller";
                var result = await _service.Register(dto);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ LOGIN
        [HttpPost("login")]
        public async Task<IActionResult> Login([FromBody] LoginDto dto)
        {
            try
            {
                var token = await _service.Login(dto);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ VERIFY OTP (FIXED)
        [HttpPost("verify-otp")]
        public async Task<IActionResult> VerifyOtp([FromBody] VerifyOtpDto dto)
        {
            try
            {
                var result = await _service.VerifyOtp(dto.Email, dto.Otp);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ LOGOUT
        [HttpPost("logout")]
        public IActionResult Logout()
        {
            return Ok("Logged out successfully");
        }

        // ✅ FORGOT PASSWORD (FIXED)
        [HttpPost("forgot-password")]
        public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordDto dto)
        {
            try
            {
                var token = await _service.ForgotPassword(dto.Email);
                return Ok(new { token });
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ RESET PASSWORD (BETTER WAY)
        [HttpPost("reset-password")]
        public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordDto dto)
        {
            try
            {
                var result = await _service.ResetPassword(dto.Token, dto.NewPassword);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return BadRequest(ex.Message);
            }
        }

        // ✅ AUTH APIs
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