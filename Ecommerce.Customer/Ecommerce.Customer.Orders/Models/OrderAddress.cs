namespace Ecommerce.Customer.OrderAPI.Models
{
    // Owned entity — stored as columns inside the Orders table (no separate FK join)
    public class OrderAddress
    {
        public int Id { get; set; }
        public int OrderId { get; set; }
        public Order Order { get; set; }

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
        public string? Label { get; set; }          // "Home", "Office"
    }
}