using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.DTOs.Job;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController : BaseController
    {
        private readonly JobService _jobService;

        public JobsController(JobService jobService)
        {
            _jobService = jobService;
        }

        #region Jobs

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
