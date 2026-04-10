using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.MockInterview;
using Microsoft.Extensions.Logging;

namespace AIInterview.Application.Services
{
    public class MockInterviewService
    {
        private readonly IMockInterviewRepository _repo;
        private readonly IAiService _aiService;
        private readonly IUserSkillRepository _skillRepo;
        private readonly ILogger<MockInterviewService> _logger;

        public MockInterviewService(
            IMockInterviewRepository repo,
            IAiService aiService,
            IUserSkillRepository skillRepo,
            ILogger<MockInterviewService> logger)
        {
            _repo      = repo;
            _aiService = aiService;
            _skillRepo = skillRepo;
            _logger    = logger;
        }

        public async Task<StartInterviewResultDto> StartSessionAsync(string userId, List<string>? providedSkills)
        {
            var skills = providedSkills?.Where(s => !string.IsNullOrWhiteSpace(s)).ToList();

            if (skills == null || skills.Count == 0)
            {
                var dbSkills = await _skillRepo.GetByUserIdAsync(userId);
                skills = dbSkills.Select(s => s.Name).ToList();
            }

            if (skills.Count == 0)
                throw new InvalidOperationException("No skills found. Please add skills to your profile or provide them manually.");

            _logger.LogInformation("Starting mock interview session for user {UserId} with skills: {Skills}", userId, string.Join(", ", skills));

            var sessionId     = await _repo.CreateSessionAsync(userId, skills);
            var firstQuestion = await _aiService.ConductInterview(skills, [], null);
            await _repo.AddMessageAsync(sessionId, "ai", firstQuestion);

            _logger.LogInformation("Mock interview session {SessionId} created for user {UserId}", sessionId, userId);

            return new StartInterviewResultDto
            {
                SessionId     = sessionId,
                Skills        = skills,
                FirstQuestion = firstQuestion
            };
        }

        public async Task<InterviewTurnResultDto> SendMessageAsync(string userId, Guid sessionId, string userMessage)
        {
            var session = await _repo.GetSessionAsync(sessionId, userId)
                ?? throw new InvalidOperationException("Session not found.");

            if (session.Status == "completed")
                throw new InvalidOperationException("This interview session has already been completed.");

            await _repo.AddMessageAsync(sessionId, "user", userMessage);

            var history = session.Messages
                .Select(m => (m.Role, m.Content))
                .ToList();
            history.Add(("user", userMessage));

            var aiResponse = await _aiService.ConductInterview(session.Skills, history, null);

            bool isCompleted   = aiResponse.Contains("[INTERVIEW_COMPLETE]", StringComparison.OrdinalIgnoreCase);
            string cleanResponse = aiResponse.Replace("[INTERVIEW_COMPLETE]", "").Trim();

            await _repo.AddMessageAsync(sessionId, "ai", cleanResponse);

            if (isCompleted)
            {
                await _repo.CompleteSessionAsync(sessionId, userId);
                _logger.LogInformation("Mock interview session {SessionId} completed for user {UserId}", sessionId, userId);
            }

            return new InterviewTurnResultDto
            {
                AiMessage       = cleanResponse,
                IsCompleted     = isCompleted,
                FeedbackSummary = isCompleted ? cleanResponse : null
            };
        }

        public async Task<InterviewSessionDto?> GetSessionAsync(string userId, Guid sessionId)
            => await _repo.GetSessionAsync(sessionId, userId);

        public async Task<IEnumerable<InterviewSessionDto>> GetSessionsAsync(string userId)
            => await _repo.GetSessionsByUserAsync(userId);
    }
}
