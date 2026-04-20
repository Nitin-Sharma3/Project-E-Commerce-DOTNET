using ProductAPI.Models;

namespace ProductAPI.Services;

public interface IProductService
{
    Task<IEnumerable<Product>> GetAll();

    Task<Product> GetById(int id);

    Task<Product> Add(Product product);

    Task<Product> Update(Product product);

    Task<bool> Delete(int id);
}