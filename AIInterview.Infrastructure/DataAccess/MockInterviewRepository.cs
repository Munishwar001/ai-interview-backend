using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.MockInterview;
using Dapper;
using System.Data;
using System.Text.Json;

namespace AIInterview.Infrastructure.DataAccess
{
    public class MockInterviewRepository : IMockInterviewRepository
    {
        private readonly IDbConnection _db;

        public MockInterviewRepository(IDbConnection db)
        {
            _db = db;
        }

        // Flat row that Dapper maps directly — avoids dynamic casing issues
        private class SessionRow
        {
            public Guid Id { get; set; }
            public string UserId { get; set; } = string.Empty;
            public string SkillsJson { get; set; } = "[]";
            public string Status { get; set; } = string.Empty;
            public DateTime CreatedAt { get; set; }
            public DateTime UpdatedAt { get; set; }
        }

        private const string SessionSelectSql = @"
            SELECT
                id              AS Id,
                user_id         AS UserId,
                skills::text    AS SkillsJson,
                status          AS Status,
                created_at      AS CreatedAt,
                updated_at      AS UpdatedAt
            FROM mock_interview_sessions";

        public async Task<Guid> CreateSessionAsync(string userId, List<string> skills)
        {
            var sql = @"
            INSERT INTO mock_interview_sessions (user_id, skills, status, created_at, updated_at)
            VALUES (@UserId, @Skills::jsonb, 'active', CURRENT_TIMESTAMP, CURRENT_TIMESTAMP)
            RETURNING id;";

            return await _db.ExecuteScalarAsync<Guid>(sql, new
            {
                UserId = userId,
                Skills = JsonSerializer.Serialize(skills)
            });
        }

        public async Task<InterviewSessionDto?> GetSessionAsync(Guid sessionId, string userId)
        {
            var sql = SessionSelectSql + " WHERE id = @SessionId AND user_id = @UserId LIMIT 1;";
            var row = await _db.QueryFirstOrDefaultAsync<SessionRow>(sql, new { SessionId = sessionId, UserId = userId });
            if (row == null) return null;

            var session = MapSession(row);
            session.Messages = (await GetMessagesAsync(sessionId)).ToList();
            return session;
        }

        public async Task<IEnumerable<InterviewSessionDto>> GetSessionsByUserAsync(string userId)
        {
            var sql = SessionSelectSql + " WHERE user_id = @UserId ORDER BY created_at DESC;";
            var rows = await _db.QueryAsync<SessionRow>(sql, new { UserId = userId });

            var sessions = new List<InterviewSessionDto>();
            foreach (var row in rows)
            {
                var session = MapSession(row);
                session.Messages = (await GetMessagesAsync(row.Id)).ToList();
                sessions.Add(session);
            }
            return sessions;
        }

        public async Task AddMessageAsync(Guid sessionId, string role, string content)
        {
            await _db.ExecuteAsync(@"
            INSERT INTO mock_interview_messages (session_id, role, content, created_at)
            VALUES (@SessionId, @Role, @Content, CURRENT_TIMESTAMP);",
            new { SessionId = sessionId, Role = role, Content = content });

            await _db.ExecuteAsync(
                "UPDATE mock_interview_sessions SET updated_at = CURRENT_TIMESTAMP WHERE id = @SessionId;",
                new { SessionId = sessionId });
        }

        public async Task CompleteSessionAsync(Guid sessionId, string userId)
        {
            await _db.ExecuteAsync(@"
            UPDATE mock_interview_sessions
            SET status = 'completed', updated_at = CURRENT_TIMESTAMP
            WHERE id = @SessionId AND user_id = @UserId;",
            new { SessionId = sessionId, UserId = userId });
        }

        public async Task<IEnumerable<InterviewMessageDto>> GetMessagesAsync(Guid sessionId)
        {
            return await _db.QueryAsync<InterviewMessageDto>(@"
            SELECT id AS Id, role AS Role, content AS Content, created_at AS CreatedAt
            FROM mock_interview_messages
            WHERE session_id = @SessionId
            ORDER BY created_at ASC;",
            new { SessionId = sessionId });
        }

        private static InterviewSessionDto MapSession(SessionRow row) => new()
        {
            Id        = row.Id,
            UserId    = row.UserId,
            Skills    = JsonSerializer.Deserialize<List<string>>(row.SkillsJson) ?? [],
            Status    = row.Status,
            CreatedAt = row.CreatedAt,
            UpdatedAt = row.UpdatedAt,
            Messages  = []
        };
    }
}
