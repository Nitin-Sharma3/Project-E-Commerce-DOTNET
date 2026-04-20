namespace Ecommerece.Customer.Address.DTOs
{
    public class UpdateAddressDto
    {
        public int Id { get; set; }

        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public AddressType Type { get; set; }

        public string Label { get; set; }

        public bool IsPrimary { get; set; }
    }
}
