using AIInterview.Core.Comman;

namespace AIInterview.Application.Interface
{
    public interface ILookupRepository
    {
        Task<IEnumerable<dynamic>> GetJobTypesAsync();
        Task<IEnumerable<LookupDto>> GetSkillsAsync();
        Task<IEnumerable<LookupDto>> GetCompanySizesAsync();
    }
}