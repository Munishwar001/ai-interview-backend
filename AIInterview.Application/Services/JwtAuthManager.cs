using AIInterview.Application.Interface;
using AIInterview.Core.Comman;
using AIInterview.Core.DTOs.Auth;
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
        public JwtAuthManager(
            IEncryptionService encryptionService,
            IUserRepository userRepository,
            IOptions<JwtConfig> jwtOptions)
        {
            _encryptionService = encryptionService;
            _userRepository = userRepository;
            _jwtConfig = jwtOptions.Value;
        }
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

                List<Claim> roleClaims = [];

                foreach (var userRole in userRoles)
                {
                    roleClaims.Add(new Claim(ClaimTypes.Role, _encryptionService.Encrypt(userRole.RoleName)));
                }
                claims.AddRange(roleClaims);

                var signinCredentials = new SigningCredentials(new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)), SecurityAlgorithms.HmacSha256);

                var expiration = DateTime.UtcNow.AddMinutes(_jwtConfig.AccessTokenExpiration);

                var jwtToken = new JwtSecurityToken(
                    issuer: _jwtConfig.Issuer,
                    audience: _jwtConfig.Audience,
                    claims: claims,
                    expires: expiration,
                    signingCredentials: signinCredentials
                );
                var tokenString = new JwtSecurityTokenHandler().WriteToken(jwtToken);

                var refreshToken = await GenerateRefreshToken(userId, oldRefreshToken);

                return new JwtAuthResult
                {
                    AccessToken = tokenString,
                    AccessTokenExpiration = expiration,
                    RefreshToken = refreshToken
                };
            }
            catch (Exception)
            {
                throw;
            }
        }

        private async Task<string> GenerateRefreshToken(string userId, string oldRefreshToken)
        {
            try
            {
                string newRefreshToken = _encryptionService.GenerateRandomToken();
                // Add new and delete the old refresh token for the user
                await _userRepository.AddNewDeleteOldUserRefreshToken(userId, newRefreshToken, oldRefreshToken, DateTime.UtcNow, DateTime.UtcNow.AddMinutes(_jwtConfig.RefreshTokenExpiration));

                return newRefreshToken;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public string? GetUserIdFromAccessToken(string accessToken)
        {
            try
            {
                var tokenValidationParameters = new TokenValidationParameters
                {
                    ValidateAudience = true,
                    ValidAudience = _jwtConfig.Audience,
                    ValidateIssuer = true,
                    ValidIssuer = _jwtConfig.Issuer,
                    ValidateIssuerSigningKey = true,
                    IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_jwtConfig.SecretKey)),
                    ValidateLifetime = false // Do not validate lifetime here
                };

                var tokenHandler = new JwtSecurityTokenHandler();
                var principal = tokenHandler.ValidateToken(accessToken, tokenValidationParameters, out SecurityToken securityToken);
                JwtSecurityToken? jwtSecurityToken = securityToken as JwtSecurityToken;
                if (jwtSecurityToken is null || !jwtSecurityToken.Header.Alg.Equals(SecurityAlgorithms.HmacSha256, StringComparison.InvariantCultureIgnoreCase))
                {
                    return null;
                }

                string? userId = principal.FindFirst(ClaimTypes.NameIdentifier)?.Value;

                if (string.IsNullOrEmpty(userId))
                {
                    return null;
                }

                return _encryptionService.Decrypt(userId);
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> ValidateRefreshToken(string userId, string refreshToken)
        {
            try
            {
                UserRefreshToken storedRefreshToken = await _userRepository.GetRefreshToken(userId, refreshToken);
                if (storedRefreshToken == null)
                {
                    return false;
                }


                // Ensure that the refresh token that we got from storage is not yet expired.
                if (DateTime.UtcNow > storedRefreshToken.ExpiresAt)
                {
                    // Delete from db if expired
                    await _userRepository.DeleteUserRefreshToken(userId, refreshToken);
                    return false;
                }
                return true;
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<bool> RevokeRefreshToken(string userId, string refreshToken)
        {
            try
            {
                return await _userRepository.DeleteUserRefreshToken(userId, refreshToken);
            }
            catch (Exception)
            {
                throw;
            }
        }
    }
}
