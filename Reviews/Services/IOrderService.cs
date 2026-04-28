namespace Reviews.Services
{
    public interface IOrderService
    {
        Task<bool> HasUserPurchasedProduct(int userId, int productId);
    }
}
