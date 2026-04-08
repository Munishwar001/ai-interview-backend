using AIInterview.Core.DTOs.MockInterview;

namespace AIInterview.Application.Interface
{
    public interface IMockInterviewRepository
    {
        Task<Guid> CreateSessionAsync(string userId, List<string> skills);
        Task<InterviewSessionDto?> GetSessionAsync(Guid sessionId, string userId);
        Task<IEnumerable<InterviewSessionDto>> GetSessionsByUserAsync(string userId);
        Task AddMessageAsync(Guid sessionId, string role, string content);
        Task CompleteSessionAsync(Guid sessionId, string userId);
        Task<IEnumerable<InterviewMessageDto>> GetMessagesAsync(Guid sessionId);
    }
}
