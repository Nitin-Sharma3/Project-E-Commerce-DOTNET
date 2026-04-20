using SellerMVC.Models;
using System.Text;
using System.Text.Json;

namespace SellerMVC.Services;

public class ProductService
{
    private readonly HttpClient _http;

    public ProductService(HttpClient http)
    {
        _http = http;
    }

    public async Task<List<ProductViewModel>> GetProducts()
    {
        var response = await _http.GetAsync("https://localhost:7163/api/Product");

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<List<ProductViewModel>>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }
    public async Task<ProductViewModel> GetProductById(int id)
    {
        var response = await _http.GetAsync($"https://localhost:7163/api/Product/{id}");

        var json = await response.Content.ReadAsStringAsync();

        return JsonSerializer.Deserialize<ProductViewModel>(json,
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
    }

    public async Task UpdateProduct(ProductViewModel product)
    {
        var json = JsonSerializer.Serialize(product);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await _http.PutAsync($"https://localhost:7163/api/Product/{product.Id}", content);
    }
    public async Task<(int totalProducts, int totalStock, int lowStock)> GetDashboardStats()
    {
        var products = await GetProducts();

        int totalProducts = products.Count;

        int totalStock = products.Sum(p => p.Stock);

        int lowStock = products.Count(p => p.Stock < 5);

        return (totalProducts, totalStock, lowStock);
    }

    public async Task AddProduct(ProductViewModel product)
    {
        var json = JsonSerializer.Serialize(product);

        var content = new StringContent(json, Encoding.UTF8, "application/json");

        await _http.PostAsync("https://localhost:7163/api/Product", content);
    }

    public async Task DeleteProduct(int id)
    {
        await _http.DeleteAsync($"https://localhost:7163/api/Product/{id}");
    }

}