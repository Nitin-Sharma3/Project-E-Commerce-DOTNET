namespace OrderAPI.HttpClients
{
    public interface IDeliveryClient
    {
        Task CreateDeliveryAsync(int orderId, int userId);
    }
}
