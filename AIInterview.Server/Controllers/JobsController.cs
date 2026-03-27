using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.DTOs.Job;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class JobsController :BaseController
    {
        private readonly JobService _jobService;

        public JobsController(JobService jobService)
        {
            _jobService = jobService;
        }

        [HttpPost]
        public async Task<IActionResult> CreateJob([FromBody] CreateJobDto request)
        {
            request.EmployerId = CurrentUserID;
            var jobId = await _jobService.CreateJobAsync(request);
            return Ok(new { jobId });
        }

        [HttpPost("generate-description")]
        public async Task<IActionResult> GenerateDescription([FromBody] GenerateDescriptionDto request,[FromServices] IAiService aiService)
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

        //[HttpGet]
        //public async Task<IActionResult> GetAllJobs()
        //{
        //    return Ok(await _jobService.GetAllJobsAsync());
        //}

        //[HttpGet("{id}")]
        //public async Task<IActionResult> GetJob(int id)
        //{
        //    var job = await _jobService.GetJobByIdAsync(id);
        //    if (job == null) return NotFound();
        //    return Ok(job);
        //}

        //[HttpGet("my-jobs/{employerId}")]
        //public async Task<IActionResult> GetMyJobs(string employerId)
        //{
        //    return Ok(await _jobService.GetJobsByEmployerAsync(employerId));
        //}
    }
}
