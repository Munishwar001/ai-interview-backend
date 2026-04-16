using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.Constants;
using AIInterview.Core.DTOs.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = AppRoles.Employer)]
    public class JobsController : BaseController
    {
        private readonly JobService _jobService;
        private readonly ILogger<JobsController> _logger;

        public JobsController(JobService jobService, ILogger<JobsController> logger)
        {
            _jobService = jobService;
            _logger = logger;
        }

        #region My Jobs

        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            try
            {
                _logger.LogInformation("GetMyJobs called by user {UserId}", CurrentUserID);
                var result = await _jobService.GetMyJobsAsync(CurrentUserID);
                _logger.LogInformation("GetMyJobs returned {Count} jobs for user {UserId}", result.Count(), CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyJobs failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto request)
        {
            try
            {
                request.EmployerId = CurrentUserID;
                var jobId = await _jobService.CreateJobAsync(request);
                _logger.LogInformation("Job {JobId} created by user {UserId}", jobId, CurrentUserID);
                return Ok(new { jobId });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CreateJob failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateJob(int id, [FromBody] UpdateJobDto request)
        {
            try
            {
                var success = await _jobService.UpdateJobAsync(id, CurrentUserID, request);
                if (!success) return CustomProblem400("Job not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateJob failed for job {JobId}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteJob(int id)
        {
            try
            {
                var success = await _jobService.DeleteJobAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Job not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteJob failed for job {JobId}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPatch("{id}/close")]
        public async Task<IActionResult> CloseJob(int id)
        {
            try
            {
                var success = await _jobService.CloseJobAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Job not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "CloseJob failed for job {JobId}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPatch("{id}/reopen")]
        public async Task<IActionResult> ReopenJob(int id)
        {
            try
            {
                var success = await _jobService.ReopenJobAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Job not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "ReopenJob failed for job {JobId}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("{id}/applicants")]
        public async Task<IActionResult> GetApplicants(int id)
        {
            try
            {
                var result = await _jobService.GetJobApplicantsAsync(id, CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetApplicants failed for job {JobId}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion

        #region AI

        [HttpPost("generate-description")]
        public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionDto request, [FromServices] IAiService aiService)
        {
            try
            {
                var prompt = $@"
                    Generate a professional job description.

                    Role: {request.Title}
                    Skills: {string.Join(", ", request.Skills)}

                    Include:
                    - Responsibilities
                    - Requirements
                    - Keep it under 150 words
                    ";

                var result = await aiService.GenerateJobDescription(prompt);
                return Ok(new { description = result });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GenerateDescription failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion
    }
}
