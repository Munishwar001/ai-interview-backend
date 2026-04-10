using AIInterview.Application.Services;
using AIInterview.Core.DTOs.MockInterview;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/mock-interview")]
    [Authorize]
    public class MockInterviewController : BaseController
    {
        private readonly MockInterviewService _service;
        private readonly ILogger<MockInterviewController> _logger;

        public MockInterviewController(MockInterviewService service, ILogger<MockInterviewController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartInterviewDto request)
        {
            try
            {
                _logger.LogInformation("MockInterview Start for user {UserId}", CurrentUserID);
                var result = await _service.StartSessionAsync(CurrentUserID, request.Skills);
                _logger.LogInformation("MockInterview session {SessionId} started for user {UserId}", result.SessionId, CurrentUserID);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MockInterview Start failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("message")]
        public async Task<IActionResult> SendMessage([FromBody] SendMessageDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.UserMessage))
                    return CustomProblem400("Message cannot be empty.");

                var result = await _service.SendMessageAsync(CurrentUserID, request.SessionId, request.UserMessage);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "MockInterview SendMessage failed for user {UserId}, session {SessionId}", CurrentUserID, request.SessionId);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            try
            {
                var session = await _service.GetSessionAsync(CurrentUserID, sessionId);
                if (session == null) return CustomProblem400("Session not found.");
                return Ok(session);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSession failed for user {UserId}, session {SessionId}", CurrentUserID, sessionId);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var sessions = await _service.GetSessionsAsync(CurrentUserID);
                return Ok(sessions);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSessions failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }
    }
}
