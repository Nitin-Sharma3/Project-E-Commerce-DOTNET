using System.ComponentModel.DataAnnotations;

namespace JAVLN.Web.Models
{
    public class ForgotPasswordViewModel
    {
        [Required, EmailAddress] public string Email { get; set; } = null!;
    }

    public class ResetPasswordViewModel
    {
        [Required] public string Token { get; set; } = null!;
        [Required, MinLength(6)] public string NewPassword { get; set; } = null!;
    }
}