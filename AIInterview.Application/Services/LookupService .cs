using AIInterview.Application.Interface;
using AIInterview.Core.Comman;

namespace AIInterview.Application.Services
{
    public class LookupService 
    {
        private readonly ILookupRepository _repository;

        public LookupService(ILookupRepository repository)
        {
            _repository = repository;
        }

        public async Task<IEnumerable<dynamic>> GetJobTypesAsync()
        {
            return await _repository.GetJobTypesAsync();
        }

        public async Task<IEnumerable<LookupDto>> GetSkillsAsync()
        {
            return await _repository.GetSkillsAsync();
        }

    }
}
