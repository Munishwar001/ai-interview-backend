namespace AIInterview.Core.DTOs.Job
{
    public class ScheduleVideoInterviewDto
    {
        public DateTime ScheduledAt { get; set; }
        public string? Notes { get; set; }
    }

    public class VideoInterviewDto
    {
        public int Id { get; set; }
        public int ApplicationId { get; set; }
        public int JobId { get; set; }
        public string EmployerId { get; set; } = string.Empty;
        public string UserId { get; set; } = string.Empty;

        public string? CandidateName { get; set; }
        public string? CandidateEmail { get; set; }
        public string? CompanyName { get; set; }

        public string RoomId { get; set; } = string.Empty;
        public string? Notes { get; set; }
        public DateTime ScheduledAt { get; set; }
        public string Status { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }
}
