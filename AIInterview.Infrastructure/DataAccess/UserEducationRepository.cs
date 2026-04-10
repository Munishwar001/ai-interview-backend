using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.JobSeeker;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class UserEducationRepository : IUserEducationRepository
    {
        private readonly IDbConnection _db;

        public UserEducationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<UserEducationDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT id, degree, institution,
                       field_of_study  AS FieldOfStudy,
                       start_year      AS StartYear,
                       end_year        AS EndYear,
                       is_current      AS IsCurrent,
                       description
                FROM user_education
                WHERE user_id = @UserId
                ORDER BY start_year DESC;";

                return await _db.QueryAsync<UserEducationDto>(sql, new { UserId = userId });
            }
            catch (Exception) { throw; }
        }

        public async Task<int> AddAsync(AddEducationDto dto)
        {
            try
            {
                var sql = @"
                INSERT INTO user_education
                    (user_id, degree, institution, field_of_study, start_year, end_year, is_current, description, created_at)
                VALUES
                    (@UserId, @Degree, @Institution, @FieldOfStudy, @StartYear, @EndYear, @IsCurrent, @Description, CURRENT_TIMESTAMP)
                RETURNING id;";

                return await _db.ExecuteScalarAsync<int>(sql, dto);
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateAsync(int id, string userId, AddEducationDto dto)
        {
            try
            {
                var sql = @"
                UPDATE user_education SET
                    degree          = @Degree,
                    institution     = @Institution,
                    field_of_study  = @FieldOfStudy,
                    start_year      = @StartYear,
                    end_year        = @EndYear,
                    is_current      = @IsCurrent,
                    description     = @Description,
                    updated_at      = CURRENT_TIMESTAMP
                WHERE id = @Id AND user_id = @UserId;";

                var rows = await _db.ExecuteAsync(sql, new
                {
                    dto.Degree, dto.Institution, dto.FieldOfStudy,
                    dto.StartYear, dto.EndYear, dto.IsCurrent, dto.Description,
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
                var sql = "DELETE FROM user_education WHERE id = @Id AND user_id = @UserId;";
                var rows = await _db.ExecuteAsync(sql, new { Id = id, UserId = userId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }
    }
}
