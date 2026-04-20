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
        => await _repo.GetById(id);

    public async Task<Product> Add(Product product)
        => await _repo.Add(product);

    public async Task<Product> Update(Product product)
        => await _repo.Update(product);

    public async Task<bool> Delete(int id)
        => await _repo.Delete(id);
}