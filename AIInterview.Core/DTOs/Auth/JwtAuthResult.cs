namespace AIInterview.Core.DTOs.Auth
{
    public class JwtAuthResult
    {
        public string AccessToken { get; set; }
        public DateTime AccessTokenExpiration { get; set; }
        public string RefreshToken { get; set; }
    }
}
