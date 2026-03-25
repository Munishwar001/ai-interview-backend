using AIInterview.Application.Interface;
using System.Security.Claims;

namespace AIInterview.Server.Middlewares
{
    public class ClaimsDecryptionMiddleware
    {
        private readonly RequestDelegate _next;

        public ClaimsDecryptionMiddleware(RequestDelegate next)
        {
            _next = next;
        }

        public async Task InvokeAsync(HttpContext context, IEncryptionService encryptionService) 
        {
            if (context.User?.Identity?.IsAuthenticated == true)
            {
                var claimTypes = new[]
                {
                ClaimTypes.NameIdentifier,
                ClaimTypes.Email,
                ClaimTypes.Role,
            };

                if (context.User.Identity is ClaimsIdentity identity)
                {
                    foreach (var claimType in claimTypes)
                    {
                        var claimsOfType = identity.FindAll(claimType).ToArray();

                        foreach (var claim in claimsOfType)
                        {
                            if (!string.IsNullOrWhiteSpace(claim.Value))
                            {
                                var decryptedValue = encryptionService.Decrypt(claim.Value);

                                identity.RemoveClaim(claim);
                                identity.AddClaim(new Claim(claimType, decryptedValue));
                            }
                        }
                    }
                }
            }

            await _next(context);
        }
    }

    public static class ClaimsDecryptionMiddlewareExtension
    {
        public static IApplicationBuilder DecryptClaims(
            this IApplicationBuilder builder)
        {
            return builder.UseMiddleware<ClaimsDecryptionMiddleware>();
        }
    }

}
