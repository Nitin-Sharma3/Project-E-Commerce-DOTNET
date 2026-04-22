using ProductAPI.Models;
using ProductAPI.Repositories;

namespace ProductAPI.Services;

public class ProductService : IProductService
{
    private readonly IProductRepository _repo;

    public ProductService(IProductRepository repo)
    {
        _repo = repo;
    }

    public async Task<IEnumerable<Product>> GetAll()
        => await _repo.GetAll();

    public async Task<Product> GetById(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid product id");

        return await _repo.GetById(id);
    }

    public async Task<Product> Add(Product product)
    {
        ValidateProduct(product);
        return await _repo.Add(product);
    }

    public async Task<Product> Update(Product product)
    {
        if (product.Id <= 0)
            throw new ArgumentException("Invalid product id");

        ValidateProduct(product);
        return await _repo.Update(product);
    }

    public async Task<bool> Delete(int id)
    {
        if (id <= 0)
            throw new ArgumentException("Invalid product id");

        return await _repo.Delete(id);
    }

    private void ValidateProduct(Product product)
    {
        if (product == null)
            throw new ArgumentNullException(nameof(product));

        if (string.IsNullOrWhiteSpace(product.Name))
            throw new ArgumentException("Product name is required");

        if (product.Name.Length > 150)
            throw new ArgumentException("Product name cannot exceed 150 characters");

        if (product.Price < 0)
            throw new ArgumentException("Price cannot be negative");

        if (product.Stock < 0)
            throw new ArgumentException("Stock cannot be negative");

        if (string.IsNullOrWhiteSpace(product.ImageUrl1))
            throw new ArgumentException("At least one image is required");
    }
}