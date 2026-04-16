using AIInterview.Core.DTOs.JobSeeker;

namespace AIInterview.Application.Interface
{
    public interface IUserProfileRepository
    {
        Task<UserProfileDto?> GetByUserIdAsync(string userId);
        Task<int> GetProfileViewsAsync(string userId);
        Task<bool> IncrementProfileViewsAsync(string userId);
        Task<int> InsertAsync(UpsertUserProfileDto dto);
        Task<bool> UpdateAsync(UpsertUserProfileDto dto);
        Task<bool> UpdateResumeAsync(string userId, string fileName, string filePath);
        Task<bool> DeleteResumeAsync(string userId);
        Task<bool> UpdateAvatarAsync(string userId, string avatarPath);
        Task<bool> DeleteAvatarAsync(string userId);
    }
}
