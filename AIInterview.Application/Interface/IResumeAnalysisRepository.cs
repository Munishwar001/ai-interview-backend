using AIInterview.Core.DTOs.Resume;

namespace AIInterview.Application.Interface
{
    public interface IResumeAnalysisRepository
    {
        Task<ResumeAnalysisResultDto?> GetByUserIdAsync(string userId);
        Task UpsertAsync(string userId, string resumeHash, string resumeText, string aiResponseJson);
    }
}
