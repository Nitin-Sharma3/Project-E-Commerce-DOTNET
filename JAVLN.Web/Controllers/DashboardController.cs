using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace JAVLN.Web.Controllers
{
    [Authorize]
    public class DashboardController : Controller
    {
        [Authorize(Roles = "Customer")]
        public IActionResult Customer() => View();

        [Authorize(Roles = "Seller")]
        public IActionResult Seller() => View();

        [Authorize(Roles = "Admin")]
        public IActionResult Admin() => View();
    }
}