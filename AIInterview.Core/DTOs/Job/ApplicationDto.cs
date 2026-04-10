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

    public class ApplicationChatRoomDto
    {
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string JobTitle { get; set; } = string.Empty;
        public string Status { get; set; } = string.Empty;
        public string ParticipantId { get; set; } = string.Empty;
        public string ParticipantName { get; set; } = string.Empty;
        public string? ParticipantAvatar { get; set; }
        public string? LastMessage { get; set; }
        public DateTime? LastMessageAt { get; set; }
    }

    public class ApplicationChatMessageDto
    {
        public long Id { get; set; }
        public int ApplicationId { get; set; }
        public string SenderId { get; set; } = string.Empty;
        public string SenderName { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    public class SendApplicationChatMessageDto
    {
        public string Message { get; set; } = string.Empty;
    }
}
