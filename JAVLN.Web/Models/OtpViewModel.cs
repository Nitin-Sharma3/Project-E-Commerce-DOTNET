using System.ComponentModel.DataAnnotations;

namespace JAVLN.Web.Models
{
    public class OtpViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required] public string Otp { get; set; } = null!;
    }
}