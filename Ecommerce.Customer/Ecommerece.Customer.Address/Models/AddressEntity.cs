using System.ComponentModel.DataAnnotations;

namespace Ecommerece.Customer.Address.Models
{
    public class AddressEntity
    {
        public int Id { get; set; }
        [MaxLength(100)]
        public string UserId { get; set; }
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }

        public string AddressLine1 { get; set; }
        public string AddressLine2 { get; set; }

        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }

        public AddressType Type { get; set; }

        public bool IsPrimary { get; set; }

        public string Label { get; set; } // "Home", "Office"

        public DateTime CreatedAt { get; set; }
    }
}
public enum AddressType
{
    Home = 1,
    Office = 2,
    Other = 3
}
