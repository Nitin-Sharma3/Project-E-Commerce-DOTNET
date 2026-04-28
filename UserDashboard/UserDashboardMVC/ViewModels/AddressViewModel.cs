using System.ComponentModel.DataAnnotations;

namespace UserDashboardMVC.ViewModels
{
    public class AddressViewModel
    {
        public int Id { get; set; }
        public int UserId { get; set; }

        [Required, MaxLength(100)]
        public string FullName { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string Phone { get; set; } = string.Empty;

        [Required, MaxLength(200)]
        public string AddressLine1 { get; set; } = string.Empty;

        [MaxLength(200)]
        public string? AddressLine2 { get; set; }

        [Required, MaxLength(100)]
        public string City { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string State { get; set; } = string.Empty;

        [Required, MaxLength(20)]
        public string PostalCode { get; set; } = string.Empty;

        [Required, MaxLength(100)]
        public string Country { get; set; } = string.Empty;

        public bool IsPrimary { get; set; }
        public DateTime CreatedAt { get; set; }
    }

    public class AddressPageViewModel
    {
        public List<AddressViewModel> Addresses { get; set; } = new();
        public AddressViewModel NewAddress { get; set; } = new();

        // Edit state — null means no card is in edit mode
        public AddressViewModel? EditAddress { get; set; }
        public int? EditAddressId { get; set; }

        public string? ErrorMessage { get; set; }
        public string? SuccessMessage { get; set; }
        public int UserId { get; set; } = 1;
    }

    public class PostalLookupResult
    {
        public string PostalCode { get; set; } = string.Empty;
        public string City { get; set; } = string.Empty;
        public string State { get; set; } = string.Empty;
        public string Country { get; set; } = string.Empty;
        public bool Found { get; set; }
    }
}