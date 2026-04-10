using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Resume;
using Dapper;
using System.Data;
using System.Text.Json;

namespace AIInterview.Infrastructure.DataAccess
{
    public class ResumeAnalysisRepository : IResumeAnalysisRepository
    {
        private readonly IDbConnection _db;

        public ResumeAnalysisRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<ResumeAnalysisResultDto?> GetByUserIdAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT
                    id::text        AS id,
                    user_id         AS userid,
                    ai_response::text AS airesponse,
                    created_at      AS createdat,
                    updated_at      AS updatedat
                FROM resume_analysis
                WHERE user_id = @UserId
                LIMIT 1;";

                var row = await _db.QueryFirstOrDefaultAsync(sql, new { UserId = userId });
                if (row == null) return null;

                var aiRaw = (string?)row.airesponse;

                return new ResumeAnalysisResultDto
                {
                    Id         = Guid.Parse((string)row.id),
                    UserId     = (string)row.userid,
                    AiResponse = string.IsNullOrEmpty(aiRaw)
                                    ? null
                                    : JsonSerializer.Deserialize<object>(aiRaw),
                    CreatedAt  = (DateTime)row.createdat,
                    UpdatedAt  = (DateTime)row.updatedat
                };
            }
            catch (Exception) { throw; }
        }

        public async Task UpsertAsync(string userId, string resumeHash, string resumeText, string aiResponseJson)
        {
            try
            {
                // Strip null bytes and characters invalid for PostgreSQL UTF8
                resumeText     = resumeText.Replace("\0", "");
                aiResponseJson = aiResponseJson.Replace("\0", "");

                var sql = @"
                INSERT INTO resume_analysis (user_id, resume_hash, resume_text, ai_response, updated_at)
                VALUES (@UserId, @ResumeHash, @ResumeText, @AiResponse::jsonb, CURRENT_TIMESTAMP)
                ON CONFLICT (user_id) DO UPDATE SET
                    resume_hash = EXCLUDED.resume_hash,
                    resume_text = EXCLUDED.resume_text,
                    ai_response = EXCLUDED.ai_response,
                    updated_at  = CURRENT_TIMESTAMP;";

                await _db.ExecuteAsync(sql, new
                {
                    UserId       = userId,
                    ResumeHash   = resumeHash,
                    ResumeText   = resumeText,
                    AiResponse   = aiResponseJson
                });
            }
            catch (Exception) { throw; }
        }
    }
}
