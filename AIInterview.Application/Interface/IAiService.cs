namespace AIInterview.Application.Interface
{
    public interface IAiService
    {
        Task<string> GenerateJobDescription(string prompt);
    }
}