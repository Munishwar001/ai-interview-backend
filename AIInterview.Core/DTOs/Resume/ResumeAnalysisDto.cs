using Microsoft.AspNetCore.Http;

namespace AIInterview.Core.DTOs.Resume
{
    public class ResumeAnalysisResultDto
    {
        public Guid Id { get; set; }
        public string? UserId { get; set; }
        public object? AiResponse { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
    }

    public class AnalyzeResumeRequestDto
    {
        // Optional: if not provided, profile resume is used
        public IFormFile? ResumeFile { get; set; }
    }
}
