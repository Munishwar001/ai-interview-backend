namespace AIInterview.Core.DTOs.Job
{
    public class ApplyJobDto
    {
        public string? CoverLetter { get; set; }
    }

    public class ApplicationDto
    {
        public int Id { get; set; }
        public int JobId { get; set; }
        public string? JobTitle { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? Location { get; set; }
        public string? Status { get; set; }
        public string? CoverLetter { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class ApplicantDto
    {
        public int Id { get; set; }
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? ResumeFilePath { get; set; }
        public string? ResumeFileName { get; set; }
        public string? CoverLetter { get; set; }
        public string? Status { get; set; }
        public DateTime AppliedAt { get; set; }
    }

    public class UpdateApplicationStatusDto
    {
        public string Status { get; set; } = string.Empty; // Pending | Shortlisted | Rejected | Hired
    }
}
