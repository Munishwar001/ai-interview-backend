using AIInterview.Application.Services;
using AIInterview.Core.Constants;
using AIInterview.Core.DTOs.User;
using AIInterview.Infrastructure.Models;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [Authorize]
    [Route("api/[controller]")]
    [ApiController]
    public class ProfileController(UserManager<ApplicationUser> userManager, UserService userService) : BaseController
    {
        [HttpGet]
        [Authorize]
        public async Task<IActionResult> GetUserDetails()
        {
            try
            {
                var user = await userManager.FindByIdAsync(CurrentUserID);
                if (user == null)
                {
                    return CustomUnauthorized401(message: "Invalid User.", errorCategory: ErrorCategory.USER_DETAILS_401);
                }

                var userRoles = await userService.GetUserRoles(CurrentUserID);
                if (userRoles == null || !userRoles.Any())
                {
                    return CustomProblem400("No role has been assigned to the user.");
                }

                var userType = userRoles.First().RoleName;

                return Ok(new UserDTO
                {
                    Email = user.Email,
                    FullName = user.FullName,
                    UserRole = userType,
                });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message, ex);
            }
        }
    }
}
