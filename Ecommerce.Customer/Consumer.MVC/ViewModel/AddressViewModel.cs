namespace Consumer.MVC.ViewModel
{
    public class AddressViewModel
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

        public string Type { get; set; }   // "Home", "Office"
        public string Label { get; set; }

        public bool IsPrimary { get; set; }
    }
}
