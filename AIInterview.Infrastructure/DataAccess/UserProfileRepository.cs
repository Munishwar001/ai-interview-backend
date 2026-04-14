using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.JobSeeker;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class UserProfileRepository : IUserProfileRepository
    {
        private readonly IDbConnection _db;

        public UserProfileRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<UserProfileDto?> GetByUserIdAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT
                    up.id, up.name, up.title, up.location, up.email, up.avatar, up.initial,
                    profile_completion  AS ProfileCompletion,
                    COALESCE((to_jsonb(up) ->> 'profile_views')::int, 0) AS ProfileViews,
                    up.resume_file_name    AS ResumeFileName,
                    up.resume_file_path    AS ResumeFilePath,
                    up.linkedin            AS LinkedIn,
                    up.github              AS GitHub,
                    up.website,
                    up.created_at          AS CreatedAt,
                    up.updated_at          AS UpdatedAt
                FROM user_profiles up
                WHERE up.user_id = @UserId
                LIMIT 1;";

                return await _db.QueryFirstOrDefaultAsync<UserProfileDto>(sql, new { UserId = userId });
            }
            catch (Exception) { throw; }
        }

        public async Task<int> GetProfileViewsAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT COALESCE((to_jsonb(up) ->> 'profile_views')::int, 0)
                FROM user_profiles up
                WHERE up.user_id = @UserId
                LIMIT 1;";
                var views = await _db.ExecuteScalarAsync<int?>(sql, new { UserId = userId });
                return views ?? 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> IncrementProfileViewsAsync(string userId)
        {
            try
            {
                var hasColumnSql = @"
                SELECT EXISTS (
                    SELECT 1
                    FROM information_schema.columns
                    WHERE table_schema = 'public'
                      AND table_name = 'user_profiles'
                      AND column_name = 'profile_views'
                );";

                var hasColumn = await _db.ExecuteScalarAsync<bool>(hasColumnSql);
                if (!hasColumn) return false;

                var sql = @"
                UPDATE user_profiles
                SET profile_views = COALESCE(profile_views, 0) + 1,
                    updated_at = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new { UserId = userId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<int> InsertAsync(UpsertUserProfileDto dto)
        {
            try
            {
                var sql = @"
                INSERT INTO user_profiles
                    (user_id, name, title, location, email, avatar, initial, linkedin, github, website, profile_completion, created_at)
                VALUES
                    (@UserId, @Name, @Title, @Location, @Email, @Avatar, @Initial, @LinkedIn, @GitHub, @Website, @ProfileCompletion, CURRENT_TIMESTAMP)
                RETURNING id;";

                return await _db.ExecuteScalarAsync<int>(sql, dto);
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateAsync(UpsertUserProfileDto dto)
        {
            try
            {
                var sql = @"
                UPDATE user_profiles SET
                    name                = @Name,
                    title               = @Title,
                    location            = @Location,
                    email               = @Email,
                    avatar              = @Avatar,
                    initial             = @Initial,
                    linkedin            = @LinkedIn,
                    github              = @GitHub,
                    website             = @Website,
                    profile_completion  = @ProfileCompletion,
                    updated_at          = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, dto);
                return rows > 0;
            }
            catch (Exception) { throw; }
        }
        public async Task<bool> UpdateResumeAsync(string userId, string fileName, string filePath)
        {
            try
            {
                var sql = @"
                UPDATE user_profiles SET
                    resume_file_name = @FileName,
                    resume_file_path = @FilePath,
                    updated_at       = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new { UserId = userId, FileName = fileName, FilePath = filePath });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteResumeAsync(string userId)
        {
            try
            {
                var sql = @"
                UPDATE user_profiles SET
                    resume_file_name = NULL,
                    resume_file_path = NULL,
                    updated_at       = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new { UserId = userId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }
        public async Task<bool> UpdateAvatarAsync(string userId, string avatarPath)
        {
            try
            {
                var sql = @"
                UPDATE user_profiles SET
                    avatar     = @AvatarPath,
                    updated_at = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new { UserId = userId, AvatarPath = avatarPath });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteAvatarAsync(string userId)
        {
            try
            {
                var sql = @"
                UPDATE user_profiles SET
                    avatar     = NULL,
                    updated_at = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new { UserId = userId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }
    }
}
