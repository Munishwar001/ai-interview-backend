using System.ComponentModel.DataAnnotations;

namespace AIInterview.Core.DTOs.Auth
{
    public class ForgotPasswordReq
    {
        [Required, EmailAddress]
        public string Email { get; set; }
    }

    public class ResetPasswordReq
    {
       public string Uid { get; set; } = null!;

        [Required]
        public string Code { get; set; }
        [Required]
        public string NewPassword { get; set; }
    }
}
