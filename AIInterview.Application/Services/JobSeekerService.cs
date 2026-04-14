using AIInterview.Application.Interface;
using AIInterview.Core.Comman;
using AIInterview.Core.DTOs.JobSeeker;

namespace AIInterview.Application.Services
{
    public class JobSeekerService
    {
        private readonly IUserProfileRepository _profileRepo;
        private readonly IUserExperienceRepository _experienceRepo;
        private readonly IUserEducationRepository _educationRepo;
        private readonly IUserSkillRepository _skillRepo;

        public JobSeekerService(
            IUserProfileRepository profileRepo,
            IUserExperienceRepository experienceRepo,
            IUserEducationRepository educationRepo,
            IUserSkillRepository skillRepo)
        {
            _profileRepo = profileRepo;
            _experienceRepo = experienceRepo;
            _educationRepo = educationRepo;
            _skillRepo = skillRepo;
        }

        #region Profile

        public async Task<UserProfileDto?> GetProfileAsync(string userId)
        {
            try { return await _profileRepo.GetByUserIdAsync(userId); }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpsertProfileAsync(UpsertUserProfileDto dto)
        {
            try
            {
                var existing = await _profileRepo.GetByUserIdAsync(dto.UserId!);
                bool hasResume = existing?.ResumeFilePath != null;
                var skills = await _skillRepo.GetByUserIdAsync(dto.UserId!);
                bool hasSkills = skills.Any();
                dto.ProfileCompletion = CalculateProfileCompletion(dto, hasResume, hasSkills);

                if (existing == null)
                {
                    await _profileRepo.InsertAsync(dto);
                    return true;
                }
                return await _profileRepo.UpdateAsync(dto);
            }
            catch (Exception) { throw; }
        }

        public async Task<int> GetProfileViewsAsync(string userId)
        {
            try { return await _profileRepo.GetProfileViewsAsync(userId); }
            catch (Exception) { throw; }
        }

        public async Task<bool> IncrementProfileViewsAsync(string userId)
        {
            try { return await _profileRepo.IncrementProfileViewsAsync(userId); }
            catch (Exception) { throw; }
        }

        private static int CalculateProfileCompletion(UpsertUserProfileDto dto, bool hasResume = false, bool hasSkills = false)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(dto.Name),
                !string.IsNullOrWhiteSpace(dto.Title),
                !string.IsNullOrWhiteSpace(dto.Location),
                !string.IsNullOrWhiteSpace(dto.Email),
                !string.IsNullOrWhiteSpace(dto.Avatar),
                !string.IsNullOrWhiteSpace(dto.LinkedIn),
                !string.IsNullOrWhiteSpace(dto.GitHub),
                !string.IsNullOrWhiteSpace(dto.Website),
                hasResume,
                hasSkills
            };

            int filled = fields.Count(f => f);
            return (int)Math.Round((double)filled / fields.Length * 100);
        }

        public async Task<bool> UpdateAvatarAsync(string userId, string avatarPath)
        {
            try
            {
                var result = await _profileRepo.UpdateAvatarAsync(userId, avatarPath);

                if (result)
                {
                    var profile = await _profileRepo.GetByUserIdAsync(userId);
                    if (profile != null)
                    {
                        var dto = new UpsertUserProfileDto
                        {
                            UserId   = userId,
                            Name     = profile.Name,
                            Title    = profile.Title,
                            Location = profile.Location,
                            Email    = profile.Email,
                            Avatar   = avatarPath,
                            Initial  = profile.Initial,
                            LinkedIn = profile.LinkedIn,
                            GitHub   = profile.GitHub,
                            Website  = profile.Website
                        };
                        bool hasResume = !string.IsNullOrEmpty(profile.ResumeFilePath);
                        var skills = await _skillRepo.GetByUserIdAsync(userId);
                        dto.ProfileCompletion = CalculateProfileCompletion(dto, hasResume, skills.Any());
                        await _profileRepo.UpdateAsync(dto);
                    }
                }

                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteAvatarAsync(string userId)
        {
            try
            {
                var profile = await _profileRepo.GetByUserIdAsync(userId);
                if (profile == null || string.IsNullOrEmpty(profile.Avatar))
                    return false;

                var result = await _profileRepo.DeleteAvatarAsync(userId);

                if (result)
                {
                    var dto = new UpsertUserProfileDto
                    {
                        UserId   = userId,
                        Name     = profile.Name,
                        Title    = profile.Title,
                        Location = profile.Location,
                        Email    = profile.Email,
                        Avatar   = null,
                        Initial  = profile.Initial,
                        LinkedIn = profile.LinkedIn,
                        GitHub   = profile.GitHub,
                        Website  = profile.Website
                    };
                    bool hasResume = !string.IsNullOrEmpty(profile.ResumeFilePath);
                    var skills = await _skillRepo.GetByUserIdAsync(userId);
                    dto.ProfileCompletion = CalculateProfileCompletion(dto, hasResume, skills.Any());
                    await _profileRepo.UpdateAsync(dto);
                }

                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteResumeAsync(string userId)
        {
            try
            {
                var profile = await _profileRepo.GetByUserIdAsync(userId);
                if (profile == null || string.IsNullOrEmpty(profile.ResumeFilePath))
                    return false;

                var result = await _profileRepo.DeleteResumeAsync(userId);

                if (result)
                {
                    var dto = new UpsertUserProfileDto
                    {
                        UserId   = userId,
                        Name     = profile.Name,
                        Title    = profile.Title,
                        Location = profile.Location,
                        Email    = profile.Email,
                        Avatar   = profile.Avatar,
                        Initial  = profile.Initial,
                        LinkedIn = profile.LinkedIn,
                        GitHub   = profile.GitHub,
                        Website  = profile.Website
                    };
                    var skills = await _skillRepo.GetByUserIdAsync(userId);
                    dto.ProfileCompletion = CalculateProfileCompletion(dto, hasResume: false, hasSkills: skills.Any());
                    await _profileRepo.UpdateAsync(dto);
                }

                return result;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateResumeAsync(string userId, string fileName, string filePath)
        {
            try
            {
                var result = await _profileRepo.UpdateResumeAsync(userId, fileName, filePath);

                // Resume uploaded — recalculate completion
                var profile = await _profileRepo.GetByUserIdAsync(userId);
                if (profile != null)
                {
                    var dto = new UpsertUserProfileDto
                    {
                        UserId = userId,
                        Name = profile.Name,
                        Title = profile.Title,
                        Location = profile.Location,
                        Email = profile.Email,
                        Avatar = profile.Avatar,
                        Initial = profile.Initial,
                        LinkedIn = profile.LinkedIn,
                        GitHub = profile.GitHub,
                        Website = profile.Website
                    };
                    dto.ProfileCompletion = CalculateProfileCompletion(dto, hasResume: true);
                    await _profileRepo.UpdateAsync(dto);
                }

                return result;
            }
            catch (Exception) { throw; }
        }

        #endregion

        #region Experience

        public async Task<IEnumerable<UserExperienceDto>> GetExperiencesAsync(string userId)
        {
            try { return await _experienceRepo.GetByUserIdAsync(userId); }
            catch (Exception) { throw; }
        }

        public async Task<int> AddExperienceAsync(AddExperienceDto dto)
        {
            try { return await _experienceRepo.AddAsync(dto); }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateExperienceAsync(int id, string userId, AddExperienceDto dto)
        {
            try { return await _experienceRepo.UpdateAsync(id, userId, dto); }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteExperienceAsync(int id, string userId)
        {
            try { return await _experienceRepo.DeleteAsync(id, userId); }
            catch (Exception) { throw; }
        }

        #endregion

        #region Education

        public async Task<IEnumerable<UserEducationDto>> GetEducationAsync(string userId)
        {
            try { return await _educationRepo.GetByUserIdAsync(userId); }
            catch (Exception) { throw; }
        }

        public async Task<int> AddEducationAsync(AddEducationDto dto)
        {
            try { return await _educationRepo.AddAsync(dto); }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateEducationAsync(int id, string userId, AddEducationDto dto)
        {
            try { return await _educationRepo.UpdateAsync(id, userId, dto); }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteEducationAsync(int id, string userId)
        {
            try { return await _educationRepo.DeleteAsync(id, userId); }
            catch (Exception) { throw; }
        }

        #endregion

        #region Skills

        public async Task<IEnumerable<LookupDto>> GetSkillsAsync(string userId)
        {
            try { return await _skillRepo.GetByUserIdAsync(userId); }
            catch (Exception) { throw; }
        }

        public async Task SyncSkillsAsync(string userId, IEnumerable<int> skillIds)
        {
            try
            {
                await _skillRepo.SyncAsync(userId, skillIds);

                // Recalculate completion after skills change
                var profile = await _profileRepo.GetByUserIdAsync(userId);
                if (profile != null)
                {
                    var dto = new UpsertUserProfileDto
                    {
                        UserId    = userId,
                        Name      = profile.Name,
                        Title     = profile.Title,
                        Location  = profile.Location,
                        Email     = profile.Email,
                        Avatar    = profile.Avatar,
                        Initial   = profile.Initial,
                        LinkedIn  = profile.LinkedIn,
                        GitHub    = profile.GitHub,
                        Website   = profile.Website
                    };
                    bool hasResume = !string.IsNullOrEmpty(profile.ResumeFilePath);
                    bool hasSkills = skillIds.Any();
                    dto.ProfileCompletion = CalculateProfileCompletion(dto, hasResume, hasSkills);
                    await _profileRepo.UpdateAsync(dto);
                }
            }
            catch (Exception) { throw; }
        }

        #endregion
    }
}
