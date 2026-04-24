using JAVLN.Web.Models;
using JAVLN.Web.Services;
using Microsoft.AspNetCore.Authentication;
using Microsoft.AspNetCore.Authentication.Cookies;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;

namespace JAVLN.Web.Controllers
{
    public class AccountController : Controller
    {
        private readonly UserApiService _api;

        public AccountController(UserApiService api)
        {
            _api = api;
        }

        // ─── REGISTER ───────────────────────────────────────────────
        [HttpGet] public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _api.Register(new
            {
                model.Name,
                model.Email,
                model.Password,
                model.Phone
            });

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View(model);
            }

            TempData["Email"] = model.Email;
            TempData["Success"] = "Registered! Check your OTP.";
            return RedirectToAction("VerifyOtp");
        }

        // ─── REGISTER SELLER ────────────────────────────────────────
        [HttpGet] public IActionResult RegisterSeller() => View("Register");

        [HttpPost]
        public async Task<IActionResult> RegisterSeller(RegisterViewModel model)
        {
            if (!ModelState.IsValid) return View("Register", model);

            var (success, message) = await _api.RegisterSeller(new
            {
                model.Name,
                model.Email,
                model.Password,
                model.Phone,
                Role = "Seller"
            });

            if (!success)
            {
                ModelState.AddModelError("", message);
                return View("Register", model);
            }

            TempData["Email"] = model.Email;
            TempData["Success"] = "Seller registered! Verify your OTP.";
            return RedirectToAction("VerifyOtp");
        }

        // ─── VERIFY OTP ──────────────────────────────────────────────
        [HttpGet]
        public IActionResult VerifyOtp()
        {
            var model = new OtpViewModel
            {
                Email = TempData["Email"]?.ToString() ?? ""
            };
            return View(model);
        }

        [HttpPost]
        public async Task<IActionResult> VerifyOtp(OtpViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _api.VerifyOtp(model.Email, model.Otp);

            if (!success)
            {
                ModelState.AddModelError("", "Invalid or expired OTP.");
                return View(model);
            }

            TempData["Success"] = "Email verified! Please login.";
            return RedirectToAction("Login");
        }

        // ─── LOGIN ───────────────────────────────────────────────────
        [HttpGet] public IActionResult Login() => View();

        [HttpPost]
        public async Task<IActionResult> Login(LoginViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, tokenJson) = await _api.Login(new
            {
                model.Email,
                model.Password
            });

            if (!success)
            {
                ModelState.AddModelError("", "Invalid credentials or unverified email.");
                return View(model);
            }

            // Parse JWT token from response { "token": "..." }
            var tokenObj = JsonConvert.DeserializeObject<dynamic>(tokenJson);
            string jwtToken = tokenObj?.token?.ToString() ?? tokenJson;

            // Decode JWT to extract claims
            var handler = new JwtSecurityTokenHandler();
            var jwt = handler.ReadJwtToken(jwtToken);

            var role = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Role
                || c.Type == "http://schemas.microsoft.com/ws/2008/06/identity/claims/role")?.Value
                ?? "Customer";
            var email = jwt.Claims.FirstOrDefault(c => c.Type == ClaimTypes.Email
                || c.Type == "http://schemas.xmlsoap.org/ws/2005/05/identity/claims/emailaddress")?.Value
                ?? model.Email;

            // Sign in with Cookie auth
            var claims = new List<Claim>
            {
                new Claim(ClaimTypes.Email, email),
                new Claim(ClaimTypes.Role, role),
                new Claim("jwt", jwtToken)
            };

            var identity = new ClaimsIdentity(claims, CookieAuthenticationDefaults.AuthenticationScheme);
            var principal = new ClaimsPrincipal(identity);

            await HttpContext.SignInAsync(CookieAuthenticationDefaults.AuthenticationScheme, principal);

            // Store JWT in session too (for API calls from MVC)
            HttpContext.Session.SetString("jwt", jwtToken);

            // Redirect by role
            return role switch
            {
                "Admin" => RedirectToAction("Admin", "Dashboard"),
                "Seller" => RedirectToAction("Seller", "Dashboard"),
                _ => RedirectToAction("Customer", "Dashboard")
            };
        }

        // ─── FORGOT PASSWORD ─────────────────────────────────────────
        [HttpGet] public IActionResult ForgotPassword() => View();

        [HttpPost]
        public async Task<IActionResult> ForgotPassword(ForgotPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _api.ForgotPassword(model.Email);

            TempData["Success"] = success
                ? "Reset link sent! Use the token to reset your password."
                : "Email not found.";

            return View();
        }

        [HttpGet]
        public IActionResult ResetPassword(string token)
            => View(new ResetPasswordViewModel { Token = token });

        [HttpPost]
        public async Task<IActionResult> ResetPassword(ResetPasswordViewModel model)
        {
            if (!ModelState.IsValid) return View(model);

            var (success, message) = await _api.ResetPassword(model.Token, model.NewPassword);

            if (!success)
            {
                ModelState.AddModelError("", "Invalid or expired token.");
                return View(model);
            }

            TempData["Success"] = "Password reset! Please login.";
            return RedirectToAction("Login");
        }

        // ─── LOGOUT ──────────────────────────────────────────────────
        public async Task<IActionResult> Logout()
        {
            await HttpContext.SignOutAsync(CookieAuthenticationDefaults.AuthenticationScheme);
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}