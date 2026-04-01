using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.User;

namespace AIInterview.Application.Services
{
    public class UserService(IUserRepository userRepository)
    {
        public async Task<IEnumerable<UserRole>> GetUserRoles(string userID)
        {
            try
            {
                return await userRepository.GetUserRoles(userID);
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
