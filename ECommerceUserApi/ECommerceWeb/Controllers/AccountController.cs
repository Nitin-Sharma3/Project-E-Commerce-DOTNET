using ECommerceWeb.Models;
using ECommerceWeb.Services;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json.Linq;

namespace ECommerceWeb.Controllers
{
    public class AccountController : Controller
    {
        private readonly ApiService _api;

        public AccountController(ApiService api)
        {
            _api = api;
        }

        public IActionResult Login() => View();
        public IActionResult Register() => View();

        [HttpPost]
        public async Task<IActionResult> Register(RegisterModel model)
        {
            var result = await _api.Register(model);
            ViewBag.Message = result;
            return View();
        }

        [HttpPost]
        public async Task<IActionResult> Login(LoginModel model)
        {
            var res = await _api.Login(model);

            var token = JObject.Parse(res)["token"]?.ToString();

            HttpContext.Session.SetString("JWT", token);

            return RedirectToAction("Index", "Dashboard");
        }

        public IActionResult Logout()
        {
            HttpContext.Session.Clear();
            return RedirectToAction("Login");
        }
    }
}