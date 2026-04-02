using AIInterview.Core.DTOs.JobSeeker;

namespace AIInterview.Application.Interface
{
    public interface IUserEducationRepository
    {
        Task<IEnumerable<UserEducationDto>> GetByUserIdAsync(string userId);
        Task<int> AddAsync(AddEducationDto dto);
        Task<bool> UpdateAsync(int id, string userId, AddEducationDto dto);
        Task<bool> DeleteAsync(int id, string userId);
    }
}
