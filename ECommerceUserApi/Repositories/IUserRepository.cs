using ECommerceUserApi.Models;

namespace ECommerceUserApi.Repositories
{
    public interface IUserRepository
    {
        Task<User?> GetByEmail(string email);
        Task<User> Create(User user);
        Task<User?> GetByResetToken(string token);
        Task Update(User user);
    }
}