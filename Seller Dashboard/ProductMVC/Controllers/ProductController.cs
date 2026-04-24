using Microsoft.AspNetCore.Mvc;
using SellerMVC.Models;
using SellerMVC.Services;
using System.Text.Json;
using static System.Net.WebRequestMethods;

namespace SellerMVC.Controllers;

public class ProductController : Controller
{
    private readonly ProductService _service;

    public ProductController(ProductService service)
    {
        _service = service;
    }

    public async Task<IActionResult> Inventory()
    {
        var products = await _service.GetProducts();

        return View(products);
    }

    public IActionResult Add()
    {
        return View();
    }

    [HttpPost]

    public async Task<IActionResult> Add(ProductViewModel product)
    {
        await _service.AddProduct(product);

        return RedirectToAction("Inventory");
    }

    public async Task<IActionResult> Delete(int id)
    {
        await _service.DeleteProduct(id);

        return RedirectToAction("Inventory");
    }

    public async Task<IActionResult> Edit(int id)
    {
        var product = await _service.GetProductById(id);

        return View(product);
    }
    [HttpPost]
    public async Task<IActionResult> Edit(ProductViewModel product)
    {
        await _service.UpdateProduct(product);

        return RedirectToAction("Inventory");
    }
    //public async Task<IActionResult> Delete(int id)
    //{
    //    await _service.DeleteProduct(id);

    //    return RedirectToAction("Inventory");
    //}

}