namespace AIInterview.Core.Comman
{
    public class JwtConfig
    {
        public string SecretKey { set; get; }
        public string Issuer { set; get; }
        public string Audience { set; get; }
        public short AccessTokenExpiration { set; get; }
        public short RefreshTokenExpiration { set; get; }
    }
}
