using AIInterview.Core.DTOs;
using AIInterview.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController(UserManager<ApplicationUser> userManager) : BaseController
    {
        [ApiExplorerSettings(IgnoreApi = true)]
        [HttpPost("register")]
        public async Task<ActionResult<string>> Register(RegisterModel registerDto)
        {
            var user = new ApplicationUser
            {
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
                await userManager.AddToRoleAsync(user, "User");
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

    }
}
