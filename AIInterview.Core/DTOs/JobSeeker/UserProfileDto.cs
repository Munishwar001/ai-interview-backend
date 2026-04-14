namespace AIInterview.Core.DTOs.JobSeeker
{
    public class UserProfileDto
    {
        public int Id { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Initial { get; set; }
        public int ProfileCompletion { get; set; }
        public string? ResumeFileName { get; set; }
        public string? ResumeFilePath { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Website { get; set; }
        public int ProfileViews { get; set; }
        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpsertUserProfileDto
    {
        public string? UserId { get; set; }
        public string? Name { get; set; }
        public string? Title { get; set; }
        public string? Location { get; set; }
        public string? Email { get; set; }
        public string? Avatar { get; set; }
        public string? Initial { get; set; }
        public string? LinkedIn { get; set; }
        public string? GitHub { get; set; }
        public string? Website { get; set; }
        public int ProfileCompletion { get; set; }
    }
}
