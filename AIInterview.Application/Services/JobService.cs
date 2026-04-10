using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Job;

namespace AIInterview.Application.Services
{
    public class JobService
    {
        private readonly IJobRepository _jobRepository;

        public JobService(IJobRepository jobRepository)
        {
            _jobRepository = jobRepository;
        }

        public async Task<int> CreateJobAsync(CreateJobDto request)
        {
            try
            {
                if (string.IsNullOrEmpty(request.Title))
                    throw new ArgumentException("Title is required");
                return await _jobRepository.CreateJobAsync(request);
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateJobAsync(int id, string employerId, UpdateJobDto request)
        {
            try { return await _jobRepository.UpdateJobAsync(id, employerId, request); }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteJobAsync(int id, string employerId)
        {
            try { return await _jobRepository.DeleteJobAsync(id, employerId); }
            catch (Exception) { throw; }
        }

        public async Task<bool> CloseJobAsync(int id, string employerId)
        {
            try { return await _jobRepository.CloseJobAsync(id, employerId); }
            catch (Exception) { throw; }
        }

        public async Task<bool> ReopenJobAsync(int id, string employerId)
        {
            try { return await _jobRepository.ReopenJobAsync(id, employerId); }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<PostedJobDto>> GetMyJobsAsync(string employerId)
        {
            try { return await _jobRepository.GetMyJobsAsync(employerId); }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<object>> GetJobApplicantsAsync(int jobId, string employerId)
        {
            try { return await _jobRepository.GetJobApplicantsAsync(jobId, employerId); }
            catch (Exception) { throw; }
        }
    }
}
