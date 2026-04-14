using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/applications")]
    [Authorize]
    public class ApplicationController : BaseController
    {
        private readonly IApplicationRepository _repo;
        private readonly ILogger<ApplicationController> _logger;

        public ApplicationController(IApplicationRepository repo, ILogger<ApplicationController> logger)
        {
            _repo = repo;
            _logger = logger;
        }

        // ── Public job browsing ──────────────────────────────────────────

        [AllowAnonymous]
        [HttpGet("jobs")]
        public async Task<IActionResult> GetJobs(
            [FromQuery] string? search,
            [FromQuery] string? location,
            [FromQuery] int? jobTypeId)
        {
            try
            {
                var jobs = await _repo.GetPublicJobsAsync(search, location, jobTypeId);
                return Ok(jobs);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        [AllowAnonymous]
        [HttpGet("jobs/{jobId}")]
        public async Task<IActionResult> GetJob(int jobId)
        {
            try
            {
                var job = await _repo.GetPublicJobByIdAsync(jobId);
                if (job == null) return CustomProblem400("Job not found.");
                return Ok(job);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Get latest active jobs (default: 3).</summary>
        [AllowAnonymous]
        [HttpGet("jobs/latest")]
        public async Task<IActionResult> GetLatestJobs([FromQuery] int limit = 3)
        {
            try
            {
                var safeLimit = limit <= 0 ? 3 : Math.Min(limit, 20);
                var jobs = await _repo.GetLatestJobsAsync(safeLimit);
                return Ok(jobs);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Get jobs recommended based on the user's skills.</summary>
        [HttpGet("jobs/recommended")]
        public async Task<IActionResult> GetRecommended()
        {
            try
            {
                var jobs = await _repo.GetRecommendedJobsAsync(CurrentUserID);
                return Ok(jobs);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }        // ── Job seeker ───────────────────────────────────────────────────

        /// <summary>Apply to a job.</summary>
        [HttpPost("jobs/{jobId}/apply")]
        public async Task<IActionResult> Apply(int jobId, [FromBody] ApplyJobDto request)
        {
            try
            {
                if (await _repo.HasAppliedAsync(jobId, CurrentUserID))
                    return CustomProblem400("You have already applied to this job.");

                var id = await _repo.ApplyAsync(jobId, CurrentUserID, request.CoverLetter);
                return Ok(new { applicationId = id });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Check if current user has applied to a job.</summary>
        [HttpGet("jobs/{jobId}/has-applied")]
        public async Task<IActionResult> HasApplied(int jobId)
        {
            try
            {
                var applied = await _repo.HasAppliedAsync(jobId, CurrentUserID);
                return Ok(new { hasApplied = applied });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Get all applications submitted by the current user.</summary>
        [HttpGet("my")]
        public async Task<IActionResult> GetMyApplications()
        {
            try
            {
                var result = await _repo.GetMyApplicationsAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Get chat rooms for current user where application status is Shortlisted or Hired.</summary>
        [HttpGet("chat/rooms")]
        public async Task<IActionResult> GetChatRooms()
        {
            try
            {
                var rooms = await _repo.GetChatRoomsAsync(CurrentUserID);
                return Ok(rooms);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Get chat messages for a specific application if current user can access it.</summary>
        [HttpGet("{applicationId}/chat/messages")]
        public async Task<IActionResult> GetChatMessages(int applicationId)
        {
            try
            {
                var canAccess = await _repo.CanAccessApplicationChatAsync(applicationId, CurrentUserID);
                if (!canAccess) return CustomUnauthorized401("You are not allowed to access this chat.");

                var messages = await _repo.GetChatMessagesAsync(applicationId, CurrentUserID);
                return Ok(messages);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Send a chat message for a shortlisted/hired application conversation.</summary>
        [HttpPost("{applicationId}/chat/messages")]
        public async Task<IActionResult> SendChatMessage(int applicationId, [FromBody] SendApplicationChatMessageDto request)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(request.Message))
                    return CustomProblem400("Message is required.");

                var created = await _repo.AddChatMessageAsync(applicationId, CurrentUserID, request.Message);
                if (created == null)
                    return CustomUnauthorized401("You are not allowed to send messages in this chat.");

                return Ok(created);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Withdraw an application.</summary>
        [HttpDelete("{applicationId}")]
        public async Task<IActionResult> Withdraw(int applicationId)
        {
            try
            {
                var success = await _repo.WithdrawAsync(applicationId, CurrentUserID);
                if (!success) return CustomProblem400("Application not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        // ── Employer ─────────────────────────────────────────────────────

        /// <summary>Get all applicants for a job (employer only).</summary>
        [HttpGet("jobs/{jobId}/applicants")]
        public async Task<IActionResult> GetApplicants(int jobId)
        {
            try
            {
                var result = await _repo.GetApplicantsByJobAsync(jobId, CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Update applicant status: Pending | Shortlisted | Rejected | Hired</summary>
        [HttpPatch("{applicationId}/status")]
        public async Task<IActionResult> UpdateStatus(int applicationId, [FromBody] UpdateApplicationStatusDto request)
        {
            try
            {
                var allowed = new[] { "Pending", "Shortlisted", "Rejected", "Hired" };
                if (!allowed.Contains(request.Status))
                    return CustomProblem400($"Invalid status. Allowed: {string.Join(", ", allowed)}");

                var success = await _repo.UpdateStatusAsync(applicationId, CurrentUserID, request.Status);
                if (!success) return CustomProblem400("Application not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Schedule video interview for a shortlisted applicant (employer only).</summary>
        [HttpPost("{applicationId}/interviews")]
        public async Task<IActionResult> ScheduleInterview(int applicationId, [FromBody] ScheduleVideoInterviewDto request)
        {
            try
            {
                if (request.ScheduledAt <= DateTime.UtcNow)
                    return CustomProblem400("ScheduledAt must be a future date/time (UTC).");

                var interviewId = await _repo.ScheduleVideoInterviewAsync(
                    applicationId,
                    CurrentUserID,
                    request.ScheduledAt,
                    request.Notes);

                if (interviewId == null)
                    return CustomProblem400("Only shortlisted applicants from your posted jobs can be scheduled for interview.");

                var interview = await _repo.GetInterviewByIdAsync(interviewId.Value, CurrentUserID);
                return Ok(interview);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message, ex); }
        }

        /// <summary>Get all scheduled interviews for a job posted by current employer.</summary>
        [HttpGet("jobs/{jobId}/interviews")]
        public async Task<IActionResult> GetInterviewsByJob(int jobId)
        {
            try
            {
                var result = await _repo.GetInterviewsByJobAsync(jobId, CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInterviewsByJob failed for job {JobId}, user {UserId}", jobId, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        /// <summary>Get scheduled interviews for current job seeker account.</summary>
        [HttpGet("my/interviews")]
        public async Task<IActionResult> GetMyInterviews()
        {
            try
            {
                var result = await _repo.GetMyInterviewsAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyInterviews failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        /// <summary>Get a specific interview if current user is employer or candidate for it.</summary>
        [HttpGet("interviews/{interviewId}")]
        public async Task<IActionResult> GetInterview(int interviewId)
        {
            try
            {
                var interview = await _repo.GetInterviewByIdAsync(interviewId, CurrentUserID);
                if (interview == null) return CustomProblem400("Interview not found or unauthorized.");
                return Ok(interview);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetInterview failed for interview {InterviewId}, user {UserId}", interviewId, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }
    }
}
