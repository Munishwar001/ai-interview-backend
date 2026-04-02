using AIInterview.Application.Interface;
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
        private readonly UserProfileService _userProfileService;
        private readonly IUserProfileRepository _userProfileRepository;
        private readonly IUserExperienceRepository _experienceRepository;
        private readonly IUserEducationRepository _educationRepository;
        private readonly IUserSkillRepository _skillRepository;
        private readonly IWebHostEnvironment _env;

        public JobSeekerController(
            UserProfileService userProfileService,
            IUserProfileRepository userProfileRepository,
            IUserExperienceRepository experienceRepository,
            IUserEducationRepository educationRepository,
            IUserSkillRepository skillRepository,
            IWebHostEnvironment env)
        {
            _userProfileService = userProfileService;
            _userProfileRepository = userProfileRepository;
            _experienceRepository = experienceRepository;
            _educationRepository = educationRepository;
            _skillRepository = skillRepository;
            _env = env;
        }

        [HttpGet("profile")]
        public async Task<IActionResult> GetProfile()
        {
            try
            {
                var result = await _userProfileService.GetByUserIdAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        [HttpPut("profile")]
        public async Task<IActionResult> UpsertProfile([FromBody] UpsertUserProfileDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var success = await _userProfileService.UpsertAsync(request);
                return Ok(new { success });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }

        // Send as multipart/form-data, field: resume (pdf/doc/docx)
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
                var success = await _userProfileRepository.UpdateResumeAsync(CurrentUserID, resume.FileName, relativePath);

                return Ok(new { success, fileName = resume.FileName, filePath = relativePath });
            }
            catch (Exception ex)
            {
                return CustomProblem500(ex.Message);
            }
        }


        [HttpGet("experience")]
        public async Task<IActionResult> GetExperiences()
        {
            try
            {
                var result = await _experienceRepository.GetByUserIdAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpPost("experience")]
        public async Task<IActionResult> AddExperience([FromBody] AddExperienceDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var id = await _experienceRepository.AddAsync(request);
                return Ok(new { id });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpPut("experience/{id}")]
        public async Task<IActionResult> UpdateExperience(int id, [FromBody] AddExperienceDto request)
        {
            try
            {
                var success = await _experienceRepository.UpdateAsync(id, CurrentUserID, request);
                if (!success) return CustomProblem400("Experience not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpDelete("experience/{id}")]
        public async Task<IActionResult> DeleteExperience(int id)
        {
            try
            {
                var success = await _experienceRepository.DeleteAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Experience not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }


        [HttpGet("education")]
        public async Task<IActionResult> GetEducation()
        {
            try
            {
                var result = await _educationRepository.GetByUserIdAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpPost("education")]
        public async Task<IActionResult> AddEducation([FromBody] AddEducationDto request)
        {
            try
            {
                request.UserId = CurrentUserID;
                var id = await _educationRepository.AddAsync(request);
                return Ok(new { id });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpPut("education/{id}")]
        public async Task<IActionResult> UpdateEducation(int id, [FromBody] AddEducationDto request)
        {
            try
            {
                var success = await _educationRepository.UpdateAsync(id, CurrentUserID, request);
                if (!success) return CustomProblem400("Education not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        [HttpDelete("education/{id}")]
        public async Task<IActionResult> DeleteEducation(int id)
        {
            try
            {
                var success = await _educationRepository.DeleteAsync(id, CurrentUserID);
                if (!success) return CustomProblem400("Education not found or unauthorized.");
                return Ok(new { success });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }


        [HttpGet("skills")]
        public async Task<IActionResult> GetSkills()
        {
            try
            {
                var result = await _skillRepository.GetByUserIdAsync(CurrentUserID);
                return Ok(result);
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }

        // Sends full list of skill IDs — replaces existing
        // Body: { "skillIds": [1, 2, 3] }
        [HttpPut("skills")]
        public async Task<IActionResult> SyncSkills([FromBody] SyncSkillsDto request)
        {
            try
            {
                await _skillRepository.SyncAsync(CurrentUserID, request.SkillIds);
                return Ok(new { success = true });
            }
            catch (Exception ex) { return CustomProblem500(ex.Message); }
        }
    }
}
