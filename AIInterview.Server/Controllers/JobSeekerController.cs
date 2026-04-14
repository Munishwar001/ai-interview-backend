using AIInterview.Application.Services;
using AIInterview.Core.DTOs.JobSeeker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using AIInterview.Server.Services;

namespace AIInterview.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobSeekerController : BaseController
    {
        private readonly JobSeekerService _jobSeekerService;
        private readonly ICloudinaryFileService _cloudinaryFileService;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ILogger<JobSeekerController> _logger;

        public JobSeekerController(JobSeekerService jobSeekerService, ICloudinaryFileService cloudinaryFileService, IHttpClientFactory httpClientFactory, ILogger<JobSeekerController> logger)
        {
            _jobSeekerService = jobSeekerService;
            _cloudinaryFileService = cloudinaryFileService;
            _httpClientFactory = httpClientFactory;
            _logger           = logger;
        }

        #region Profile

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var result = await _jobSeekerService.GetProfileAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfile failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("profile/views")]
        public async Task<IActionResult> GetProfileViews()
        {
            try
            {
                var views = await _jobSeekerService.GetProfileViewsAsync(CurrentUserID);
                return Ok(new { views });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetProfileViews failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("profile/views/{userId}/increment")]
        public async Task<IActionResult> IncrementProfileViews(string userId)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(userId)) return CustomProblem400("UserId is required.");
                if (userId == CurrentUserID) return Ok(new { success = false, skipped = true });

                var success = await _jobSeekerService.IncrementProfileViewsAsync(userId);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "IncrementProfileViews failed for target {TargetUserId}", userId);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpsertProfile([FromBody] UpsertUserProfileDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var success = await _jobSeekerService.UpsertProfileAsync(request);
                _logger.LogInformation("Profile upserted for user {UserId}", CurrentUserID);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpsertProfile failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("upload-avatar")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadAvatar(IFormFile avatar)
        {
            try
            {
                if (avatar == null)
                    return CustomProblem400("Avatar file is required.");

                var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".webp" };
                var ext = Path.GetExtension(avatar.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                    return CustomProblem400("Only jpg, jpeg, png, and webp files are allowed.");

                if (avatar.Length > 2 * 1024 * 1024)
                    return CustomProblem400("File size must not exceed 2MB.");

                var existingProfile = await _jobSeekerService.GetProfileAsync(CurrentUserID);
                var oldAvatar = existingProfile?.Avatar;

                var uploadResult = await _cloudinaryFileService.UploadImageAsync(
                    avatar,
                    $"ai-interview/users/{CurrentUserID}/avatars",
                    "avatar");

                var success = await _jobSeekerService.UpdateAvatarAsync(CurrentUserID, uploadResult.Url);
                _logger.LogInformation("Avatar uploaded for user {UserId}: {Path}", CurrentUserID, uploadResult.Url);

                if (success && !string.IsNullOrWhiteSpace(oldAvatar) && !oldAvatar.Equals(uploadResult.Url, StringComparison.OrdinalIgnoreCase))
                {
                    try { await _cloudinaryFileService.DeleteAsync(oldAvatar); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete previous avatar for user {UserId}", CurrentUserID);
                    }
                }

                return Ok(new { success, avatarPath = uploadResult.Url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadAvatar failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpDelete("delete-avatar")]
        public async Task<IActionResult> DeleteAvatar()
        {
            try
            {
                var existingProfile = await _jobSeekerService.GetProfileAsync(CurrentUserID);
                var existingAvatar = existingProfile?.Avatar;

                var success = await _jobSeekerService.DeleteAvatarAsync(CurrentUserID);
                if (!success) return CustomProblem400("No avatar found to delete.");

                if (!string.IsNullOrWhiteSpace(existingAvatar))
                {
                    try { await _cloudinaryFileService.DeleteAsync(existingAvatar); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete avatar from Cloudinary for user {UserId}", CurrentUserID);
                    }
                }

                _logger.LogInformation("Avatar deleted for user {UserId}", CurrentUserID);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteAvatar failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("resume-status")]
        public async Task<IActionResult> GetResumeStatus()
        {
            try
            {
                var profile    = await _jobSeekerService.GetProfileAsync(CurrentUserID);
                var isUploaded = profile != null && !string.IsNullOrEmpty(profile.ResumeFilePath);
                return Ok(new
                {
                    isUploaded,
                    fileName = isUploaded ? profile!.ResumeFileName : null,
                    filePath = isUploaded ? profile!.ResumeFilePath  : null
                });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetResumeStatus failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("upload-resume")]
        [Consumes("multipart/form-data")]
        public async Task<IActionResult> UploadResume(IFormFile resume)
        {
            try
            {
                if (resume == null)
                    return CustomProblem400("Resume file is required.");

                var allowedExtensions = new[] { ".pdf", ".doc", ".docx" };
                var ext = Path.GetExtension(resume.FileName).ToLowerInvariant();

                if (!allowedExtensions.Contains(ext))
                    return CustomProblem400("Only pdf, doc, and docx files are allowed.");

                if (resume.Length > 5 * 1024 * 1024)
                    return CustomProblem400("File size must not exceed 5MB.");

                var existingProfile = await _jobSeekerService.GetProfileAsync(CurrentUserID);
                var oldResumePath = existingProfile?.ResumeFilePath;

                var uploadResult = await _cloudinaryFileService.UploadRawAsync(
                    resume,
                    $"ai-interview/users/{CurrentUserID}/resumes",
                    "resume");

                var success = await _jobSeekerService.UpdateResumeAsync(CurrentUserID, resume.FileName, uploadResult.Url);
                _logger.LogInformation("Resume uploaded for user {UserId}: {FileName}", CurrentUserID, resume.FileName);

                if (success && !string.IsNullOrWhiteSpace(oldResumePath) && !oldResumePath.Equals(uploadResult.Url, StringComparison.OrdinalIgnoreCase))
                {
                    try { await _cloudinaryFileService.DeleteAsync(oldResumePath); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete previous resume for user {UserId}", CurrentUserID);
                    }
                }

                return Ok(new { success, fileName = resume.FileName, filePath = uploadResult.Url });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UploadResume failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpDelete("delete-resume")]
        public async Task<IActionResult> DeleteResume()
        {
            try
            {
                var existingProfile = await _jobSeekerService.GetProfileAsync(CurrentUserID);
                var existingResumePath = existingProfile?.ResumeFilePath;

                var success = await _jobSeekerService.DeleteResumeAsync(CurrentUserID);
                if (!success) return CustomProblem400("No resume found to delete.");

                if (!string.IsNullOrWhiteSpace(existingResumePath))
                {
                    try { await _cloudinaryFileService.DeleteAsync(existingResumePath); }
                    catch (Exception ex)
                    {
                        _logger.LogWarning(ex, "Failed to delete resume from Cloudinary for user {UserId}", CurrentUserID);
                    }
                }

                _logger.LogInformation("Resume deleted for user {UserId}", CurrentUserID);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteResume failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpGet("download-resume")]
        public async Task<IActionResult> DownloadResume()
        {
            try
            {
                var profile = await _jobSeekerService.GetProfileAsync(CurrentUserID);

                if (profile == null || string.IsNullOrEmpty(profile.ResumeFilePath))
                    return CustomProblem400("No resume found.");

                if (Uri.TryCreate(profile.ResumeFilePath, UriKind.Absolute, out _))
                {
                    var client = _httpClientFactory.CreateClient();
                    using var response = await client.GetAsync(profile.ResumeFilePath, HttpCompletionOption.ResponseHeadersRead);

                    if (!response.IsSuccessStatusCode)
                    {
                        _logger.LogWarning("Unable to fetch cloud resume for user {UserId}. Status code: {StatusCode}", CurrentUserID, response.StatusCode);
                        return CustomProblem400("Resume file not found on server.");
                    }

                    var remoteContentType = response.Content.Headers.ContentType?.MediaType;
                    if (string.IsNullOrWhiteSpace(remoteContentType))
                    {
                        var extFromName = Path.GetExtension(profile.ResumeFileName ?? string.Empty).ToLowerInvariant();
                        remoteContentType = extFromName switch
                        {
                            ".pdf" => "application/pdf",
                            ".doc" => "application/msword",
                            ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                            _ => "application/octet-stream"
                        };
                    }

                    var remoteBytes = await response.Content.ReadAsByteArrayAsync();
                    var remoteDownloadName = string.IsNullOrWhiteSpace(profile.ResumeFileName)
                        ? "resume"
                        : profile.ResumeFileName;

                    return File(remoteBytes, remoteContentType, remoteDownloadName);
                }

                var webRoot = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");

                var absolutePath = Path.Combine(
                    webRoot,
                    profile.ResumeFilePath.TrimStart('/').Replace('/', Path.DirectorySeparatorChar));

                if (!System.IO.File.Exists(absolutePath))
                {
                    _logger.LogWarning("Resume file missing on disk for user {UserId}: {Path}", CurrentUserID, absolutePath);
                    return CustomProblem400("Resume file not found on server.");
                }

                var ext = Path.GetExtension(absolutePath).ToLowerInvariant();
                var contentType = ext switch
                {
                    ".pdf"  => "application/pdf",
                    ".doc"  => "application/msword",
                    ".docx" => "application/vnd.openxmlformats-officedocument.wordprocessingml.document",
                    _       => "application/octet-stream"
                };

                var fileBytes    = await System.IO.File.ReadAllBytesAsync(absolutePath);
                var downloadName = string.IsNullOrEmpty(profile.ResumeFileName)
                    ? Path.GetFileName(absolutePath)
                    : profile.ResumeFileName;

                return File(fileBytes, contentType, downloadName);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DownloadResume failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion

        #region Experience

        [HttpGet("experience")]
        public async Task<IActionResult> GetExperiences()
        {
            try
            {
                var result = await _jobSeekerService.GetExperiencesAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetExperiences failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("experience")]
        public async Task<IActionResult> AddExperience([FromBody] AddExperienceDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var id = await _jobSeekerService.AddExperienceAsync(request);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddExperience failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPut("experience/{id}")]
        public async Task<IActionResult> UpdateExperience(int id, [FromBody] AddExperienceDto request)
        {
            try
            {
                var success = await _jobSeekerService.UpdateExperienceAsync(id, CurrentUserID, request);
                if (!success) return CustomProblem400("Experience not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateExperience failed for id {Id}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpDelete("experience/{id}")]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            try
            {
                var success = await _jobSeekerService.DeleteExperienceAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Experience not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteExperience failed for id {Id}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion

        #region Education

        [HttpGet("education")]
        public async Task<IActionResult> GetEducation()
        {
            try
            {
                var result = await _jobSeekerService.GetEducationAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetEducation failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPost("education")]
        public async Task<IActionResult> AddEducation([FromBody] AddEducationDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var id = await _jobSeekerService.AddEducationAsync(request);
                return Ok(new { id });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "AddEducation failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPut("education/{id}")]
        public async Task<IActionResult> UpdateEducation(int id, [FromBody] AddEducationDto request)
        {
            try
            {
                var success = await _jobSeekerService.UpdateEducationAsync(id, CurrentUserID, request);
                if (!success) return CustomProblem400("Education not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "UpdateEducation failed for id {Id}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpDelete("education/{id}")]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            try
            {
                var success = await _jobSeekerService.DeleteEducationAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Education not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "DeleteEducation failed for id {Id}, user {UserId}", id, CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion

        #region Skills

        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            try
            {
                var result = await _jobSeekerService.GetSkillsAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "GetSkills failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        [HttpPut("skills")]
        public async Task<IActionResult> SyncSkills([FromBody] SyncSkillsDto request)
        {
            try
            {
                await _jobSeekerService.SyncSkillsAsync(CurrentUserID, request.SkillIds);
                return Ok(new { success = true });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "SyncSkills failed for user {UserId}", CurrentUserID);
                return CustomProblem500(ex.Message, ex);
            }
        }

        #endregion
    }
}
