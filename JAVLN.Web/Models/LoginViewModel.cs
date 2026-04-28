using System.ComponentModel.DataAnnotations;

namespace JAVLN.Web.Models
{
    public class LoginViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required] public string Password { get; set; } = null!;
    }
}