using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.JobSeeker;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class UserExperienceRepository : IUserExperienceRepository
    {
        private readonly IDbConnection _db;

        public UserExperienceRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserExperienceDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT id, job_title AS JobTitle, company, location,
                       start_date AS StartDate, end_date AS EndDate,
                       is_current AS IsCurrent, description
                FROM user_experiences
                WHERE user_id = @UserId
                ORDER BY start_date DESC;";

                return await _db.QueryAsync<UserExperienceDto>(sql, new { UserId = userId });
            }
            catch (Exception) { throw; }
        }

        public async Task<int> AddAsync(AddExperienceDto dto)
        {
            try
            {
                var sql = @"
                INSERT INTO user_experiences
                    (user_id, job_title, company, location, start_date, end_date, is_current, description, created_at)
                VALUES
                    (@UserId, @JobTitle, @Company, @Location, @StartDate, @EndDate, @IsCurrent, @Description, CURRENT_TIMESTAMP)
                RETURNING id;";

                return await _db.ExecuteScalarAsync<int>(sql, dto);
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateAsync(int id, string userId, AddExperienceDto dto)
        {
            try
            {
                var sql = @"
                UPDATE user_experiences SET
                    job_title   = @JobTitle,
                    company     = @Company,
                    location    = @Location,
                    start_date  = @StartDate,
                    end_date    = @EndDate,
                    is_current  = @IsCurrent,
                    description = @Description,
                    updated_at  = CURRENT_TIMESTAMP
                WHERE id = @Id AND user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new
                {
                    dto.JobTitle, dto.Company, dto.Location,
                    dto.StartDate, dto.EndDate, dto.IsCurrent, dto.Description,
                    Id = id, UserId = userId
                });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteAsync(int id, string userId)
        {
            try
            {
                var sql = "DELETE FROM user_experiences WHERE id = @Id AND user_id = @UserId;";
                var rows = await _db.ExecuteAsync(sql, new { Id = id, UserId = userId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }
    }
}
