using Ecommerce.Customer.OrderAPI.Models;

namespace Ecommerce.Customer.OrderAPI.DTOs
{
    public class PlaceOrderDto
    {
        // Either supply an existing address ID from Address API
        // OR supply a full inline address (e.g. guest checkout)
        public int? AddressId { get; set; }
        public InlineAddressDto? InlineAddress { get; set; }

        public PaymentMethod PaymentMethod { get; set; }
        public string? Notes { get; set; }
        public string? CouponCode { get; set; }
    }

    public class InlineAddressDto
    {
        public string FullName { get; set; }
        public string PhoneNumber { get; set; }
        public string AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string PostalCode { get; set; }
        public string Country { get; set; }
    }
}