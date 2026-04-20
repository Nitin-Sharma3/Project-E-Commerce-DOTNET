using Microsoft.AspNetCore.Mvc;
using SellerMVC.Services;

public class HomeController : Controller
{
    private readonly ProductService _service;

    public HomeController(ProductService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Index()
    {
        var stats = await _service.GetDashboardStats();

        ViewBag.TotalProducts = stats.totalProducts;
        ViewBag.TotalStock = stats.totalStock;
        ViewBag.LowStock = stats.lowStock;

        return View();
    }
}