using AIInterview.Core.DTOs.JobSeeker;

namespace AIInterview.Application.Interface
{
    public interface IUserProfileRepository
    {
        Task<UserProfileDto?> GetByUserIdAsync(string userId);
        Task<int> InsertAsync(UpsertUserProfileDto dto);
        Task<bool> UpdateAsync(UpsertUserProfileDto dto);
        Task<bool> UpdateResumeAsync(string userId, string fileName, string filePath);
    }
}
