using AIInterview.Core.DTOs.JobSeeker;

namespace AIInterview.Application.Interface
{
    public interface IUserExperienceRepository
    {
        Task<IEnumerable<UserExperienceDto>> GetByUserIdAsync(string userId);
        Task<int> AddAsync(AddExperienceDto dto);
        Task<bool> UpdateAsync(int id, string userId, AddExperienceDto dto);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
