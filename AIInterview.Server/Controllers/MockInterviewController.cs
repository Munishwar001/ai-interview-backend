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

        public MockInterviewController(MockInterviewService service)
        {
            _service = service;
        }

        /// <summary>
        /// Start a new mock interview session.
        /// Skills are loaded from the user's profile if not provided.
        /// </summary>
        [HttpPost("start")]
        public async Task<IActionResult> Start([FromBody] StartInterviewDto request)
        {
            try
            {
                var result = await _service.StartSessionAsync(CurrentUserID, request.Skills);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        /// <summary>
        /// Send a user answer and receive the next AI question or final feedback.
        /// </summary>
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        /// <summary>
        /// Get all messages for a specific session.
        /// </summary>
        [HttpGet("session/{sessionId}")]
        public async Task<IActionResult> GetSession(Guid sessionId)
        {
            try
            {
                var session = await _service.GetSessionAsync(CurrentUserID, sessionId);
                if (session == null) return CustomProblem400("Session not found.");
                return Ok(session);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        /// <summary>
        /// Get all interview sessions for the current user.
        /// </summary>
        [HttpGet("sessions")]
        public async Task<IActionResult> GetSessions()
        {
            try
            {
                var sessions = await _service.GetSessionsAsync(CurrentUserID);
                return Ok(sessions);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }
    }
}
