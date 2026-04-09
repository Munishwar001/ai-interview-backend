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

        public ApplicationController(IApplicationRepository repo)
        {
            _repo = repo;
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }
    }
}
