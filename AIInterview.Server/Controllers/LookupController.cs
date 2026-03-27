using AIInterview.Application.Services;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class LookupController: BaseController
    {
        private readonly LookupService _service;

        public LookupController(LookupService service)
        {
            _service = service;
        }

        [HttpGet("job-types")]
        public async Task<IActionResult> GetJobTypes()
        {
            var result = await _service.GetJobTypesAsync();
            return Ok(result);
        }

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            var result = await _service.GetSkillsAsync();
            return Ok(result);
        }

    }
}
