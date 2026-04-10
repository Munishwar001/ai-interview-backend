namespace AIInterview.Core.DTOs.Job
{
    public class CreateJobDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int? JobType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? EmployerId { get; set; }
        public List<int>? SkillIds { get; set; }
    }

    public class UpdateJobDto
    {
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int? JobTypeId { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public List<int>? SkillIds { get; set; }
    }

    public class PostedJobDto
    {
        public int Id { get; set; }
        public string? Title { get; set; }
        public string? Description { get; set; }
        public string? Location { get; set; }
        public int? JobTypeId { get; set; }
        public string? JobType { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? Status { get; set; }
        public int Applicants { get; set; }
        public int Views { get; set; }
        public int Shortlisted { get; set; }
        public DateTime CreatedAt { get; set; }
        // Company info from join
        public int? CompanyId { get; set; }
        public string? CompanyName { get; set; }
        public string? CompanyLogo { get; set; }
        public string? CompanyDescription { get; set; }
        public List<SkillTagDto> Skills { get; set; } = new();
    }

    public class SkillTagDto
    {
        public int Id { get; set; }
        public string Name { get; set; }
    }

    public class GenerateDescriptionDto
    {
        public string Title { get; set; }
        public List<string> Skills { get; set; }
    }
}
