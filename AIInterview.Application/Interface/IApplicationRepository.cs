using AIInterview.Core.DTOs.Job;

namespace AIInterview.Application.Interface
{
    public interface IApplicationRepository
    {
        // Job seeker
        Task<int> ApplyAsync(int jobId, string userId, string? coverLetter);
        Task<bool> WithdrawAsync(int applicationId, string userId);
        Task<IEnumerable<ApplicationDto>> GetMyApplicationsAsync(string userId);
        Task<bool> HasAppliedAsync(int jobId, string userId);

        // Employer
        Task<IEnumerable<ApplicantDto>> GetApplicantsByJobAsync(int jobId, string employerId);
        Task<bool> UpdateStatusAsync(int applicationId, string employerId, string status);

        // Public job browsing
        Task<IEnumerable<PostedJobDto>> GetPublicJobsAsync(string? search, string? location, int? jobTypeId);
        Task<PostedJobDto?> GetPublicJobByIdAsync(int jobId);
        Task<IEnumerable<PostedJobDto>> GetRecommendedJobsAsync(string userId);
    }
}
