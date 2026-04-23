using Microsoft.AspNetCore.Mvc;

namespace ECommerceWeb.Controllers
{
    public class DashboardController : Controller
    {
        public IActionResult Index()
        {
            var token = HttpContext.Session.GetString("JWT");

            if (string.IsNullOrEmpty(token))
                return RedirectToAction("Login", "Account");

            return View();
        }
    }
}