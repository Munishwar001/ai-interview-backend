namespace AIInterview.Core.Models
{
    public class UserRefreshToken
    {
        public int Id { get; set; }
        public string UserId { get; set; } = null!;
        public string RefreshToken { get; set; } = null!;
        public DateTime IssuedAt { get; set; }
        public DateTime ExpiresAt { get; set; }
    }
}
