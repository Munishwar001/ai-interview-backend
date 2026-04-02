using AIInterview.Application.Interface;
using AIInterview.Core.Comman;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class UserSkillRepository : IUserSkillRepository
    {
        private readonly IDbConnection _db;

        public UserSkillRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<LookupDto>> GetByUserIdAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT s.id, s.name
                FROM user_skills us
                JOIN skills s ON us.skill_id = s.id
                WHERE us.user_id = @UserId
                ORDER BY s.name;";

                return await _db.QueryAsync<LookupDto>(sql, new { UserId = userId });
            }
            catch (Exception) { throw; }
        }

        // Replaces all existing skills for the user with the new list
        public async Task SyncAsync(string userId, IEnumerable<int> skillIds)
        {
            try
            {
                await _db.ExecuteAsync(
                    "DELETE FROM user_skills WHERE user_id = @UserId;",
                    new { UserId = userId });

                foreach (var skillId in skillIds)
                {
                    await _db.ExecuteAsync(
                        "INSERT INTO user_skills (user_id, skill_id) VALUES (@UserId, @SkillId);",
                        new { UserId = userId, SkillId = skillId });
                }
            }
            catch (Exception) { throw; }
        }
    }
}
