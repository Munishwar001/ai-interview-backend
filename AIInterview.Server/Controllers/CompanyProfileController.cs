using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.DTOs.Company;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class CompanyProfileController : BaseController
    {
        private readonly CompanyService _companyService;
        private readonly IWebHostEnvironment _env;

        public CompanyProfileController(CompanyService companyService, IWebHostEnvironment env)
        {
            _companyService = companyService;
            _env = env;
        }

        [HttpGet("me")]
        public async Task<IActionResult> GetMyCompanyProfile()
        {
            try
            {
                var result = await _companyService.GetByUserIdAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        // Send as application/json
        [HttpPut]
        public async Task<IActionResult> UpsertCompanyProfile([FromBody] UpdateCompanyProfileDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var result = await _companyService.UpsertAsync(request);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        [HttpPost("generate-description")]
        public async Task<IActionResult> GenerateDescription(
            [FromBody] GenerateCompanyDescriptionDto request,
            [FromServices] IAiService aiService)
        {
            try
            {
                var prompt = $@"
                    Generate a professional company description.

                    Company Name: {request.CompanyName}
                    Industry: {request.Industry}
                    Tagline: {request.Tagline}

                    Include:
                    - What the company does
                    - Its mission or value proposition
                    - Keep it under 120 words, professional tone
                    ";

                var result = await aiService.GenerateJobDescription(prompt);
                return Ok(new { description = result });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        // Send as multipart/form-data
        // Fields: logo (file, optional), coverImage (file, optional)
        [HttpPost("upload-images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages(IFormFile? logo, IFormFile? coverImage)
        {
            try
            {
                if (logo == null && coverImage == null)
                    return CustomProblem400("At least one image (logo or coverImage) is required.");

                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "uploads", "company");
                Directory.CreateDirectory(uploadsFolder);

                string? logoUrl = logo != null ? await SaveFileAsync(logo, uploadsFolder, "logo") : null;
                string? coverUrl = coverImage != null ? await SaveFileAsync(coverImage, uploadsFolder, "cover") : null;

                var result = await _companyService.UpdateImagesAsync(CurrentUserID, logoUrl, coverUrl);
                return Ok(result);
            }
            catch (InvalidOperationException ex)
            {
                return CustomProblem400(ex.Message);
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        private async Task<string> SaveFileAsync(IFormFile file, string folder, string prefix)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' is not allowed. Use jpg, png, or webp.");

            if (file.Length > 5 * 1024 * 1024)
                throw new InvalidOperationException("File size must not exceed 5MB.");

            var fileName = $"{prefix}_{CurrentUserID}_{Guid.NewGuid()}{ext}";
            var filePath = Path.Combine(folder, fileName);

            using var stream = new FileStream(filePath, FileMode.Create);
            await file.CopyToAsync(stream);

            return $"/uploads/company/{fileName}";
        }
    }
}
