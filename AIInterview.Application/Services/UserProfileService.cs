using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.JobSeeker;

namespace AIInterview.Application.Services
{
    public class UserProfileService
    {
        private readonly IUserProfileRepository _repo;

        public UserProfileService(IUserProfileRepository repo)
        {
            _repo = repo;
        }

        public async Task<UserProfileDto?> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _repo.GetByUserIdAsync(userId);
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpsertAsync(UpsertUserProfileDto dto)
        {
            try
            {
                var existing = await _repo.GetByUserIdAsync(dto.UserId!);

                if (existing == null)
                {
                    await _repo.InsertAsync(dto);
                    return true;
                }

                return await _repo.UpdateAsync(dto);
            }
            catch (Exception) { throw; }
        }
    }
}
