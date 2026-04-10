namespace AIInterview.Core.DTOs.JobSeeker
{
    public class UserEducationDto
    {
        public int Id { get; set; }
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }

    public class AddEducationDto
    {
        public string? UserId { get; set; }
        public string Degree { get; set; } = string.Empty;
        public string Institution { get; set; } = string.Empty;
        public string? FieldOfStudy { get; set; }
        public int StartYear { get; set; }
        public int? EndYear { get; set; }
        public bool IsCurrent { get; set; }
        public string? Description { get; set; }
    }
}
