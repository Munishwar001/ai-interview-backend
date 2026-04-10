namespace AIInterview.Core.DTOs.MockInterview
{
    // Sent by client to start a new session
    public class StartInterviewDto
    {
        /// <summary>If null, skills are loaded from the user's profile.</summary>
        public List<string>? Skills { get; set; }
    }

    // Sent by client for each answer turn
    public class SendMessageDto
    {
        public Guid SessionId { get; set; }
        public string UserMessage { get; set; } = string.Empty;
    }

    // A single chat message stored / returned
    public class InterviewMessageDto
    {
        public int Id { get; set; }
        public string Role { get; set; } = string.Empty;   // "ai" | "user"
        public string Content { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; }
    }

    // Full session info
    public class InterviewSessionDto
    {
        public Guid Id { get; set; }
        public string UserId { get; set; } = string.Empty;
        public List<string> Skills { get; set; } = [];
        public string Status { get; set; } = string.Empty;  // "active" | "completed"
        public DateTime CreatedAt { get; set; }
        public DateTime UpdatedAt { get; set; }
        public List<InterviewMessageDto> Messages { get; set; } = [];
    }

    // Returned when a session is started
    public class StartInterviewResultDto
    {
        public Guid SessionId { get; set; }
        public List<string> Skills { get; set; } = [];
        public string FirstQuestion { get; set; } = string.Empty;
    }

    // Returned after each user message
    public class InterviewTurnResultDto
    {
        public string AiMessage { get; set; } = string.Empty;
        public bool IsCompleted { get; set; }
        public string? FeedbackSummary { get; set; }
    }
}
