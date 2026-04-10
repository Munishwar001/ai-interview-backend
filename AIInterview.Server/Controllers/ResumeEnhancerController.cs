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
        private readonly ILogger<ResumeEnhancerController> _logger;

        public ResumeEnhancerController(ResumeEnhancerService service, IWebHostEnvironment env, ILogger<ResumeEnhancerController> logger)
        {
            _service = service;
            _env     = env;
            _logger  = logger;
        }

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

                _logger.LogInformation("Resume analyze started for user {UserId}, file: {FileName}", CurrentUserID, resume.FileName);
                using var stream = resume.OpenReadStream();
                var result = await _service.AnalyzeAsync(CurrentUserID, stream, resume.FileName);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Analyze failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("analyze-from-profile")]
        public async Task<IActionResult> AnalyzeFromProfile()
        {
            try
            {
                _logger.LogInformation("AnalyzeFromProfile started for user {UserId}", CurrentUserID);
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var result  = await _service.AnalyzeFromProfileAsync(CurrentUserID, webRoot);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AnalyzeFromProfile failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("result")]
        public async Task<IActionResult> GetResult()
        {
            try
            {
                var result = await _service.GetCachedResultAsync(CurrentUserID);
                if (result == null) return CustomProblem400("No analysis found. Please analyze your resume first.");
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetResult failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }
    }
}
