using System.ComponentModel.DataAnnotations;

namespace AIInterview.Core.DTOs.Auth
{
    public record RefreshReq
    {
        [Required]
        public string AccessToken { get; set; }
        [Required]
        public string RefreshToken { get; set; }
    }

    public record RevokeReq
    {
        [Required]
        public string RefreshToken { get; set; }
    }

    public record UserRefreshToken
    {
        public string UserID { get; set; }
        public string RefreshToken { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
