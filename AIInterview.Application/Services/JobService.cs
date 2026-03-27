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
            if (string.IsNullOrEmpty(request.Title))
                throw new Exception("Title is required");

            return await _jobRepository.CreateJobAsync(request);
        }

    }
}
