using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.DTOs.Job;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class JobsController : BaseController
    {
        private readonly JobService _jobService;

        public JobsController(JobService jobService)
        {
            _jobService = jobService;
        }

        #region My Jobs

        [HttpGet("my-jobs")]
        public async Task<IActionResult> GetMyJobs()
        {
            try
            {
                var result = await _jobService.GetMyJobsAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto request)
        {
            try
            {
                request.EmployerId = CurrentUserID;
                var jobId = await _jobService.CreateJobAsync(request);
                return Ok(new { jobId });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpGet("{id}/applicants")]
        public async Task<IActionResult> GetApplicants(int id)
        {
            try
            {
                var result = await _jobService.GetJobApplicantsAsync(id, CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
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
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        #endregion
    }
}
