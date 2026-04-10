using AIInterview.Application.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/resume-enhancer")]
    [Authorize]
    public class ResumeEnhancerController : BaseController
    {
        private readonly ResumeEnhancerService _service;
        private readonly IWebHostEnvironment _env;

        public ResumeEnhancerController(ResumeEnhancerService service, IWebHostEnvironment env)
        {
            _service = service;
            _env     = env;
        }

        // Analyze resume uploaded directly
        [HttpPost("analyze")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> Analyze(IFormFile resume)
        {
            try
            {
                if (resume == null)
                    return CustomProblem400("Resume file is required.");

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx", ".txt" };
                var ext = Path.GetExtension(resume.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                    return CustomProblem400("Only pdf, doc, docx, and txt files are allowed.");

                if (resume.Length > 5 * 1024 * 1024)
                    return CustomProblem400("File size must not exceed 5MB.");

                using var stream = resume.OpenReadStream();
                var result = await _service.AnalyzeAsync(CurrentUserID, stream, resume.FileName);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        // Analyze resume already uploaded to profile
        [HttpPost("analyze-from-profile")]
        public async Task<IActionResult> AnalyzeFromProfile()
        {
            try
            {
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var result  = await _service.AnalyzeFromProfileAsync(CurrentUserID, webRoot);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        // Get last cached analysis result
        [HttpGet("result")]
        public async Task<IActionResult> GetResult()
        {
            try
            {
                var result = await _service.GetCachedResultAsync(CurrentUserID);
                if (result == null) return CustomProblem400("No analysis found. Please analyze your resume first.");
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }
    }
}
