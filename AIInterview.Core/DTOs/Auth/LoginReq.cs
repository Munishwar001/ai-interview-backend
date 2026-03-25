using System.ComponentModel.DataAnnotations;

namespace AIInterview.Core.DTOs.Auth
{
    public class LoginReq
    {
        [Required]
        public string Email { get; set; }
        [Required]
        public string Password { get; set; }
    }

    public class LoginResp
    {
        public string Email { get; set; }
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }
        public string RefreshToken { get; set; }
    }
}
