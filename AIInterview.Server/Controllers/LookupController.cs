using AIInterview.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController : BaseController
    {
        private readonly LookupService _service;
        private readonly ILogger<LookupController> _logger;

        public LookupController(LookupService service, ILogger<LookupController> logger)
        {
            _service = service;
            _logger  = logger;
        }

        [HttpGet("job-types")]
        public async Task<IActionResult> GetJobTypes()
        {
            try
            {
                var result = await _service.GetJobTypesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetJobTypes failed");
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            try
            {
                var result = await _service.GetSkillsAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSkills (lookup) failed");
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("company-sizes")]
        public async Task<IActionResult> GetCompanySizes()
        {
            try
            {
                var result = await _service.GetCompanySizesAsync();
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetCompanySizes failed");
                return CustomProblem500(ex.Message, ex);
            }
        }
    }
}
