using System.ComponentModel.DataAnnotations;

namespace AIInterview.Core.DTOs.Auth
{
    public class RegisterModel
    {
        [Required]
        [StringLength(100, MinimumLength = 3)]
        public string FullName { get; set; } = null!;

        [Required]
        [EmailAddress]
        public string Email { get; set; } = null!;

        [Required]
        [MinLength(6)]
        public string Password { get; set; } = null!;

        [Required]
        [RegularExpression("JobSeeker|Employer", ErrorMessage = "Invalid role")]
        public string Role { get; set; } = null!;
    }

    public class AuthResponse
    {
        public string? Token { get; set; } = string.Empty;
        public bool IsSuccess { get; set; }
        public string? Message { get; set; }
    }
}
