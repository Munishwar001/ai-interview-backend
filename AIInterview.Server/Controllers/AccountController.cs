using AIInterview.Application.Services;
using AIInterview.Core.Constants;
using AIInterview.Core.DTOs.Auth;
using AIInterview.Infrastructure.Models;
using Google.Apis.Auth;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(UserManager<ApplicationUser> userManager , JwtAuthManager jwtAuthManager , IConfiguration configuration) : BaseController
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("register")]
         public async Task<IActionResult> Register(RegisterModel registerDto)
        {
            try
            {
                var user = new ApplicationUser
                {
                    FullName = registerDto.FullName,
                    Email = registerDto.Email,
                    UserName = registerDto.Email
                };

                var result = await userManager.CreateAsync(user, registerDto.Password);

                if (!result.Succeeded)
                {
                    return Ok(new AuthResponse
                    {
                        IsSuccess = false,
                        Message = string.Join(",", result.Errors)
                    });
                }

                if (registerDto.Role is null)
                {
                    await userManager.AddToRoleAsync(user, AppRoles.JobSeeker);
                }
                else
                {
                    await userManager.AddToRoleAsync(user, registerDto.Role);
                }


                return Ok(new AuthResponse
                {
                    IsSuccess = true,
                    Message = "Account Created Sucessfully!"
                });
            }
            catch(Exception ex)
            {
                return CustomProblem500(ex.Message);
            }

        }

        [HttpPost("login")]
        public async Task<IActionResult> Login(LoginReq login)
        {

            try
            {
                var user = await userManager.FindByEmailAsync(login.Email);
                if (user is null)
                {
                    return CustomUnauthorized401(message: "Invalid email or password.", errorCategory: ErrorCategory.LOGIN_401);
                }

                var result = await userManager.CheckPasswordAsync(user, login.Password);

                if (!result)
                {
                    return CustomUnauthorized401(message: "Invalid email or password.", errorCategory: ErrorCategory.LOGIN_401);
                }

                var jwtResult = await jwtAuthManager.GenerateTokens(user.Id, user.UserName);

                return Ok(new LoginResp
                {
                    Email = login.Email,
                    AccessToken = jwtResult.AccessToken,
                    AccessTokenExpiration = jwtResult.AccessTokenExpiration,
                    RefreshToken = jwtResult.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        [HttpPost("refresh-token")]
        public async Task<IActionResult> RefreshToken(RefreshReq request)
        {
            try
            {
                string? userId = jwtAuthManager.GetUserIdFromAccessToken(request.AccessToken);
                if (string.IsNullOrEmpty(userId))
                {
                    return CustomUnauthorized401(message: "Invalid Token.", errorCategory: ErrorCategory.TOKEN_REFRESH_401);
                }

                var user = await userManager.FindByIdAsync(userId);
                if (user == null)
                {
                    return CustomUnauthorized401(message: "Invalid Token.", errorCategory: ErrorCategory.TOKEN_REFRESH_401);
                }

                if (user.LockoutEnabled && user.LockoutEnd.HasValue && user.LockoutEnd.Value > DateTimeOffset.UtcNow)
                {
                    return CustomUnauthorized401(message: "Your account is locked.", errorCategory: ErrorCategory.LOGIN_401);
                }

                bool validated = await jwtAuthManager.ValidateRefreshToken(userId, request.RefreshToken);
                if (!validated)
                {
                    return CustomUnauthorized401(message: "Invalid Token.", errorCategory: ErrorCategory.TOKEN_REFRESH_401);
                }


                var jwtResult = await jwtAuthManager.GenerateTokens(userId, user.UserName, request.RefreshToken);

                return Ok(new JwtAuthResult
                {
                    AccessToken = jwtResult.AccessToken,
                    AccessTokenExpiration = jwtResult.AccessTokenExpiration,
                    RefreshToken = jwtResult.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        [Authorize]
        [HttpPost("revoke-token")]
        public async Task<IActionResult> RevokeRefreshToken([FromBody] RevokeReq revokeRequest)
        {
            try
            {
                await jwtAuthManager.RevokeRefreshToken(CurrentUserID, revokeRequest.RefreshToken);
                return Ok();
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        [HttpPost("google-login")]
        public async Task<IActionResult> GoogleLogin(GoogleLoginDto request)
        {
            try
            {
                GoogleJsonWebSignature.Payload payload;
                try
                {
                    var settings = new GoogleJsonWebSignature.ValidationSettings
                    {
                        Audience = new[] { configuration["Google:ClientId"] }
                    };
                    payload = await GoogleJsonWebSignature.ValidateAsync(request.IdToken, settings);
                }
                catch (Exception)
                {
                    return CustomUnauthorized401(
                        message: "Invalid Google token.",
                        errorCategory: ErrorCategory.LOGIN_401
                    );
                }

                var user = await userManager.FindByEmailAsync(payload.Email);

                if (user is null)
                {
                    user = new ApplicationUser
                    {
                        UserName = payload.Email,
                        Email = payload.Email,
                        FullName = payload.Name,
                        EmailConfirmed = true  
                    };

                    var createResult = await userManager.CreateAsync(user);
                    if (!createResult.Succeeded)
                    {
                        return CustomProblem500(string.Join(", ", createResult.Errors.Select(e => e.Description)));
                    }

                    await userManager.AddToRoleAsync(user, request.Role);
                }

                var jwtResult = await jwtAuthManager.GenerateTokens(user.Id, user.UserName);

                return Ok(new LoginResp
                {
                    Email = user.Email,
                    AccessToken = jwtResult.AccessToken,
                    AccessTokenExpiration = jwtResult.AccessTokenExpiration,
                    RefreshToken = jwtResult.RefreshToken
                });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }
    }
}
