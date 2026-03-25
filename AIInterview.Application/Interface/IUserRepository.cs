using AIInterview.Core.DTOs.Auth;
using AIInterview.Core.DTOs.User;

namespace AIInterview.Application.Interface
{
    public interface IUserRepository
    {
        Task<IEnumerable<UserRole>> GetUserRoles(string userId);
        Task<bool> AddNewDeleteOldUserRefreshToken(string userId, string newRefreshToken, string oldRefreshToken, DateTime issuedAt, DateTime expiresAt);
        Task<UserRefreshToken> GetRefreshToken(string userId, string refreshToken);
        Task<bool> DeleteUserRefreshToken(string userId, string refreshToken);

    }
}