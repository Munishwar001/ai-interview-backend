using AIInterview.Core.DTOs.Job;

namespace AIInterview.Application.Interface
{
    public interface IJobRepository
    {
        Task<int> CreateJobAsync(CreateJobDto request);
        Task<IEnumerable<object>> GetAllJobsAsync();
        Task<object> GetJobByIdAsync(int id);
        Task<IEnumerable<object>> GetJobsByEmployerAsync(string employerId);
    }
}