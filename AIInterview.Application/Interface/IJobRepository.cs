using AIInterview.Core.DTOs.Job;

namespace AIInterview.Application.Interface
{
    public interface IJobRepository
    {
        Task<int> CreateJobAsync(CreateJobDto request);
        Task<bool> UpdateJobAsync(int id, string employerId, UpdateJobDto request);
        Task<bool> DeleteJobAsync(int id, string employerId);
        Task<bool> CloseJobAsync(int id, string employerId);
        Task<bool> ReopenJobAsync(int id, string employerId);
        Task<IEnumerable<PostedJobDto>> GetMyJobsAsync(string employerId);
        Task<object> GetJobByIdAsync(int id);
        Task<IEnumerable<object>> GetAllJobsAsync();
        Task<IEnumerable<object>> GetJobsByEmployerAsync(string employerId);
        Task<IEnumerable<object>> GetJobApplicantsAsync(int jobId, string employerId);
    }
}