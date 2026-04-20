namespace Ecommerece.Customer.Address.DTOs
{
    public class CreateAddressDto
    {
        public int? Id { get; set; }

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public int Type { get; set; }     // 🔥 REQUIRED
        public string Label { get; set; } // 🔥 REQUIRED

        public bool IsPrimary { get; set; }
    }
}
