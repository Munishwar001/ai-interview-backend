using AIInterview.Core.Comman;

namespace AIInterview.Application.Interface
{
    public interface IUserSkillRepository
    {
        Task<IEnumerable<LookupDto>> GetByUserIdAsync(string userId);
        Task SyncAsync(string userId, IEnumerable<int> skillIds);
    }
}
