namespace AIInterview.Application.Interface
{
    public interface IAiService
    {
        Task<string> GenerateJobDescription(string prompt);
        Task<string> AnalyzeResume(string resumeText);
        Task<string> ConductInterview(List<string> skills, List<(string role, string content)> history, string? userMessage);
    }
}