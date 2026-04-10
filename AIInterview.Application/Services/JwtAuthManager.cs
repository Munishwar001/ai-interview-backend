using AIInterview.Application.Interface;
using AIInterview.Core.Comman;
using AIInterview.Core.DTOs.Auth;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;
using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;

namespace AIInterview.Application.Services
{
    public class JwtAuthManager
    {
        private readonly IEncryptionService _encryptionService;
        private readonly IUserRepository _userRepository;
        private readonly JwtConfig _jwtConfig;
        private readonly ILogger<JwtAuthManager> _logger;

        public JwtAuthManager(
            IEncryptionService encryptionService,
            IUserRepository userRepository,
            IOptions<JwtConfig> jwtOptions,
            ILogger<JwtAuthManager> logger)
        {
            _encryptionService = encryptionService;
            _userRepository    = userRepository;
            _jwtConfig         = jwtOptions.Value;
            _logger            = logger;
        }

        #region Token Generation

        public async Task<JwtAuthResult> GenerateTokens(string userId, string email, string? oldRefreshToken = null)
        {
            try
            {
                var claims = new List<Claim>
                {
                    new(ClaimTypes.NameIdentifier, _encryptionService.Encrypt(userId)),
                    new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
                    new(ClaimTypes.Email, _encryptionService.Encrypt(email)),
                };

                var userRoles = await _userRepository.GetUserRoles(userId);
                foreach (var userRole in userRoles)
                    claims.Add(new Claim(ClaimTypes.Role, _encryptionService.Encrypt(userRole.RoleName)));

                var signinCredentials = new SigningCredentials(
                    new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)),
                    SecurityAlgorithms.HmacSha256);

                var expiration = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpiration);

                var jwtToken = new JwtSecurityToken(
                    issuer:            _jwtConfig.Issuer,
                    audience:          _jwtConfig.Audience,
                    claims:            claims,
                    expires:           expiration,
                    signingCredentials: signinCredentials);

                var tokenString  = new JwtSecurityTokenHandler().WriteToken(jwtToken);
                var refreshToken = await GenerateRefreshToken(userId, oldRefreshToken);

                _logger.LogInformation("Tokens generated for user {UserId}, expires {Expiry}", userId, expiration);

                return new JwtAuthResult
                {
                    AccessToken           = tokenString,
                    AccessTokenExpiration = expiration,
                    RefreshToken          = refreshToken
                };
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateTokens failed for user {UserId}", userId);
                throw;
            }
        }

        private async Task<string> GenerateRefreshToken(string userId, string? oldRefreshToken)
        {
            try
            {
                string newRefreshToken = _encryptionService.GenerateRandomToken();
                await _userRepository.AddNewDeleteOldUserRefreshToken(
                    userId, newRefreshToken, oldRefreshToken,
                    DateTime.UtcNow, DateTime.UtcNow.AddMinutes(_jwtConfig.RefreshTokenExpiration));
                return newRefreshToken;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateRefreshToken failed for user {UserId}", userId);
                throw;
            }
        }

        #endregion

        #region Token Validation

        public string? GetUserIdFromAccessToken(string accessToken)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience         = true,
                    ValidAudience            = _jwtConfig.Audience,
                    ValidateIssuer           = true,
                    ValidIssuer              = _jwtConfig.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey         = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)),
                    ValidateLifetime         = false
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal    = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
                var jwtToken     = securityToken as JwtSecurityToken;

                if (jwtToken is null || !jwtToken.Header.Alg.Equals(
                    SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    _logger.LogWarning("GetUserIdFromAccessToken — invalid token algorithm");
                    return null;
                }

                string? userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;
                if (string.IsNullOrEmpty(userId))
                {
                    _logger.LogWarning("GetUserIdFromAccessToken — NameIdentifier claim missing");
                    return null;
                }

                return _encryptionService.Decrypt(userId);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetUserIdFromAccessToken threw");
                throw;
            }
        }

        public async Task<bool> ValidateRefreshToken(string userId, string refreshToken)
        {
            try
            {
                var storedToken = await _userRepository.GetRefreshToken(userId, refreshToken);
                if (storedToken == null)
                {
                    _logger.LogWarning("ValidateRefreshToken — token not found for user {UserId}", userId);
                    return false;
                }

                if (DateTime.UtcNow > storedToken.ExpiresAt)
                {
                    _logger.LogWarning("ValidateRefreshToken — token expired for user {UserId}", userId);
                    await _userRepository.DeleteUserRefreshToken(userId, refreshToken);
                    return false;
                }

                return true;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ValidateRefreshToken threw for user {UserId}", userId);
                throw;
            }
        }

        public async Task<bool> RevokeRefreshToken(string userId, string refreshToken)
        {
            try
            {
                return await _userRepository.DeleteUserRefreshToken(userId, refreshToken);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "RevokeRefreshToken threw for user {UserId}", userId);
                throw;
            }
        }

        #endregion
    }
}
