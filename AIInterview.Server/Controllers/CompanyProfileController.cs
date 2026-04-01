using AIInterview.Application.Services;
using AIInterview.Core.DTOs.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyProfileController : BaseController
    {
        private readonly CompanyService _companyService;
        CompanyProfileController(CompanyService companyService) 
        {
            _companyService = companyService;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyCompanyProfile()
        {
            var result = await _companyService.GetByUserIdAsync(CurrentUserID);
            return Ok(result);
        }

        [HttpPut]
        public async Task<IActionResult> UpsertCompanyProfile([FromBody] UpdateCompanyProfileDto request)
        {
            request.UserId = CurrentUserID;

            var result = await _companyService.UpsertAsync(request);

            return Ok(result);
        }
    }
}
