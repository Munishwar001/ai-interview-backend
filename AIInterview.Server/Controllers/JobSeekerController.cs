using AIInterview.Application.Services;
using AIInterview.Core.DTOs.JobSeeker;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace AIInterview.Server.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    [Authorize]
    public class JobSeekerController : BaseController
    {
        private readonly JobSeekerService _jobSeekerService;
        private readonly IWebHostEnvironment _env;
        private readonly ILogger<JobSeekerController> _logger;

        public JobSeekerController(JobSeekerService jobSeekerService, IWebHostEnvironment env, ILogger<JobSeekerController> logger)
        {
            _jobSeekerService = jobSeekerService;
            _env              = env;
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

                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "uploads", "avatars");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"avatar_{CurrentUserID}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await avatar.CopyToAsync(stream);

                var relativePath = $"/uploads/avatars/{fileName}";
                var success = await _jobSeekerService.UpdateAvatarAsync(CurrentUserID, relativePath);
                _logger.LogInformation("Avatar uploaded for user {UserId}: {Path}", CurrentUserID, relativePath);

                return Ok(new { success, avatarPath = relativePath });
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
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var success = await _jobSeekerService.DeleteAvatarAsync(CurrentUserID, webRoot);
                if (!success) return CustomProblem400("No avatar found to delete.");
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

                var uploadsFolder = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
                    "uploads", "resumes");
                Directory.CreateDirectory(uploadsFolder);

                var fileName = $"resume_{CurrentUserID}_{Guid.NewGuid()}{ext}";
                var filePath = Path.Combine(uploadsFolder, fileName);

                using var stream = new FileStream(filePath, FileMode.Create);
                await resume.CopyToAsync(stream);

                var relativePath = $"/uploads/resumes/{fileName}";
                var success = await _jobSeekerService.UpdateResumeAsync(CurrentUserID, resume.FileName, relativePath);
                _logger.LogInformation("Resume uploaded for user {UserId}: {FileName}", CurrentUserID, resume.FileName);

                return Ok(new { success, fileName = resume.FileName, filePath = relativePath });
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
                var webRoot = _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot");
                var success = await _jobSeekerService.DeleteResumeAsync(CurrentUserID, webRoot);
                if (!success) return CustomProblem400("No resume found to delete.");
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

                var absolutePath = Path.Combine(
                    _env.WebRootPath ?? Path.Combine(Directory.GetCurrentDirectory(), "wwwroot"),
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
