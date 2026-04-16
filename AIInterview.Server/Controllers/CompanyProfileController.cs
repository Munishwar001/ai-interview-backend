using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.Constants;
using AIInterview.Core.DTOs.Company;
using AIInterview.Server.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize(Roles = AppRoles.Employer)]
    public class CompanyProfileController : BaseController
    {
        private readonly CompanyService _companyService;
        private readonly ICloudinaryFileService _cloudinaryFileService;
        private readonly ILogger<CompanyProfileController> _logger;

        public CompanyProfileController(CompanyService companyService, ICloudinaryFileService cloudinaryFileService, ILogger<CompanyProfileController> logger)
        {
            _companyService = companyService;
            _cloudinaryFileService = cloudinaryFileService;
            _logger         = logger;
        }

        #region Profile

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
                _logger.LogError(ex, "GetMyCompanyProfile failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("views")]
        public async Task<IActionResult> GetMyProfileViews()
        {
            try
            {
                var views = await _companyService.GetProfileViewsAsync(CurrentUserID);
                return Ok(new { views });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetMyProfileViews failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("views/company/{companyId:int}/increment")]
        public async Task<IActionResult> IncrementProfileViewsByCompanyId(int companyId)
        {
            try
            {
                var success = await _companyService.IncrementProfileViewsByCompanyIdAsync(companyId);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncrementProfileViewsByCompanyId failed for company {CompanyId}", companyId);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPut]
        public async Task<IActionResult> UpsertCompanyProfile([FromBody] UpdateCompanyProfileDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var result = await _companyService.UpsertAsync(request);
                _logger.LogInformation("Company profile upserted for user {UserId}", CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertCompanyProfile failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion

        #region AI

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
                _logger.LogError(ex, "GenerateDescription (company) failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion

        #region Images

        [HttpPost("upload-images")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadImages(IFormFile? logo, IFormFile? coverImage)
        {
            try
            {
                if (logo == null && coverImage == null)
                    return CustomProblem400("At least one image (logo or coverImage) is required.");

                var existingProfile = await _companyService.GetByUserIdAsync(CurrentUserID);

                string? logoUrl = null;
                if (logo != null)
                {
                    ValidateImageFile(logo, 5 * 1024 * 1024);
                    var logoUpload = await _cloudinaryFileService.UploadImageAsync(
                        logo,
                        $"ai-interview/companies/{CurrentUserID}/assets",
                        "logo");
                    logoUrl = logoUpload.Url;
                }

                string? coverUrl = null;
                if (coverImage != null)
                {
                    ValidateImageFile(coverImage, 5 * 1024 * 1024);
                    var coverUpload = await _cloudinaryFileService.UploadImageAsync(
                        coverImage,
                        $"ai-interview/companies/{CurrentUserID}/assets",
                        "cover");
                    coverUrl = coverUpload.Url;
                }

                var result = await _companyService.UpdateImagesAsync(CurrentUserID, logoUrl, coverUrl);

                if (logoUrl != null && !string.IsNullOrWhiteSpace(existingProfile?.LogoUrl) && !existingProfile.LogoUrl.Equals(logoUrl, StringComparison.OrdinalIgnoreCase))
                {
                    try { await _cloudinaryFileService.DeleteAsync(existingProfile.LogoUrl); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete previous company logo for user {UserId}", CurrentUserID);
                    }
                }

                if (coverUrl != null && !string.IsNullOrWhiteSpace(existingProfile?.CoverImageUrl) && !existingProfile.CoverImageUrl.Equals(coverUrl, StringComparison.OrdinalIgnoreCase))
                {
                    try { await _cloudinaryFileService.DeleteAsync(existingProfile.CoverImageUrl); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete previous company cover image for user {UserId}", CurrentUserID);
                    }
                }

                _logger.LogInformation("Company images uploaded for user {UserId}", CurrentUserID);
                return Ok(result);
            }
            catch (InvalidOperationException ex) { return CustomProblem400(ex.Message); }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadImages failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        private static void ValidateImageFile(IFormFile file, long maxBytes)
        {
            var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
            var ext = Path.GetExtension(file.FileName).ToLowerInvariant();

            if (!allowedExtensions.Contains(ext))
                throw new InvalidOperationException($"File type '{ext}' is not allowed. Use jpg, png, or webp.");

            if (file.Length > maxBytes)
                throw new InvalidOperationException("File size must not exceed 5MB.");
        }

        #endregion
    }
}
