using System.ComponentModel.DataAnnotations;

namespace JAVLN.Web.Models
{
    public class RegisterViewModel
    {
        [Required] public string Name { get; set; } = null!;
        [Required, EmailAddress] public string Email { get; set; } = null!;
        [Required, MinLength(6)] public string Password { get; set; } = null!;
        [Required] public string Phone { get; set; } = null!;
        public string? Role { get; set; }
    }
}