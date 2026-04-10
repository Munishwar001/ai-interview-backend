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
            try
            {
                return await _repository.GetJobTypesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LookupDto>> GetSkillsAsync()
        {
            try
            {
                return await _repository.GetSkillsAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

        public async Task<IEnumerable<LookupDto>> GetCompanySizesAsync()
        {
            try
            {
                return await _repository.GetCompanySizesAsync();
            }
            catch (Exception)
            {
                throw;
            }
        }

    }
}
