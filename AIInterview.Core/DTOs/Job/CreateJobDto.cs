namespace AIInterview.Core.DTOs.Job
{
    public class CreateJobDto
    {
        public string Title { get; set; }
        public string Description { get; set; }
        public string Location { get; set; }
        public decimal? SalaryMin { get; set; }
        public decimal? SalaryMax { get; set; }
        public string? CompanyName { get; set; } = "test";
        public string? EmployerId { get; set; }
        public List<int> SkillIds { get; set; }
    }

    public class GenerateDescriptionDto
    {
        public string Title { get; set; }
        public List<string> Skills { get; set; }
    }

}
