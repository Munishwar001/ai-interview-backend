using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Job;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class ApplicationRepository : IApplicationRepository
    {
        private readonly IDbConnection _db;

        public ApplicationRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<int> ApplyAsync(int jobId, string userId, string? coverLetter)
        {
            var sql = @"
            INSERT INTO job_applications (job_id, user_id, cover_letter, status, applied_at)
            VALUES (@JobId, @UserId, @CoverLetter, 'Pending', CURRENT_TIMESTAMP)
            RETURNING id;";

            return await _db.ExecuteScalarAsync<int>(sql, new { JobId = jobId, UserId = userId, CoverLetter = coverLetter });
        }

        public async Task<bool> WithdrawAsync(int applicationId, string userId)
        {
            var rows = await _db.ExecuteAsync(
                "DELETE FROM job_applications WHERE id = @Id AND user_id = @UserId",
                new { Id = applicationId, UserId = userId });
            return rows > 0;
        }

        public async Task<IEnumerable<ApplicationDto>> GetMyApplicationsAsync(string userId)
        {
            var sql = @"
            SELECT
                ja.id           AS Id,
                ja.job_id       AS JobId,
                j.title         AS JobTitle,
                cp.company_name AS CompanyName,
                cp.logo_url     AS CompanyLogo,
                j.location      AS Location,
                ja.status       AS Status,
                ja.cover_letter AS CoverLetter,
                ja.applied_at   AS AppliedAt
            FROM job_applications ja
            JOIN jobs j ON ja.job_id = j.id
            LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
            WHERE ja.user_id = @UserId
            ORDER BY ja.applied_at DESC;";

            return await _db.QueryAsync<ApplicationDto>(sql, new { UserId = userId });
        }

        public async Task<bool> HasAppliedAsync(int jobId, string userId)
        {
            var count = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM job_applications WHERE job_id = @JobId AND user_id = @UserId",
                new { JobId = jobId, UserId = userId });
            return count > 0;
        }

        public async Task<IEnumerable<ApplicantDto>> GetApplicantsByJobAsync(int jobId, string employerId)
        {
            // Verify ownership
            var owns = await _db.ExecuteScalarAsync<int>(
                "SELECT COUNT(1) FROM jobs WHERE id = @JobId AND employerid = @EmployerId",
                new { JobId = jobId, EmployerId = employerId });

            if (owns == 0) return [];

            var sql = @"
            SELECT
                ja.id               AS Id,
                ja.user_id          AS UserId,
                up.name             AS Name,
                up.email            AS Email,
                up.avatar           AS Avatar,
                up.resume_file_path AS ResumeFilePath,
                up.resume_file_name AS ResumeFileName,
                ja.cover_letter     AS CoverLetter,
                ja.status           AS Status,
                ja.applied_at       AS AppliedAt
            FROM job_applications ja
            LEFT JOIN user_profiles up ON up.user_id = ja.user_id
            WHERE ja.job_id = @JobId
            ORDER BY ja.applied_at DESC;";

            return await _db.QueryAsync<ApplicantDto>(sql, new { JobId = jobId });
        }

        public async Task<bool> UpdateStatusAsync(int applicationId, string employerId, string status)
        {
            // Only allow employer who owns the job to update status
            var sql = @"
            UPDATE job_applications ja
            SET status = @Status
            FROM jobs j
            WHERE ja.id = @ApplicationId
              AND ja.job_id = j.id
              AND j.employerid = @EmployerId;";

            var rows = await _db.ExecuteAsync(sql, new
            {
                ApplicationId = applicationId,
                EmployerId    = employerId,
                Status        = status
            });
            return rows > 0;
        }

        public async Task<int?> ScheduleVideoInterviewAsync(int applicationId, string employerId, DateTime scheduledAt, string? notes)
        {
            var roomId = Guid.NewGuid().ToString("N");

            var sql = @"
            INSERT INTO application_interviews
                (application_id, job_id, employer_id, user_id, room_id, notes, scheduled_at, status, created_at)
            SELECT
                ja.id,
                ja.job_id,
                j.employerid,
                ja.user_id,
                @RoomId,
                @Notes,
                @ScheduledAt,
                'Scheduled',
                CURRENT_TIMESTAMP
            FROM job_applications ja
            JOIN jobs j ON j.id = ja.job_id
            WHERE ja.id = @ApplicationId
              AND j.employerid = @EmployerId
              AND ja.status = 'Shortlisted'
            RETURNING id;";

            return await _db.QueryFirstOrDefaultAsync<int?>(sql, new
            {
                ApplicationId = applicationId,
                EmployerId = employerId,
                ScheduledAt = scheduledAt,
                RoomId = roomId,
                Notes = notes
            });
        }

        public async Task<IEnumerable<VideoInterviewDto>> GetInterviewsByJobAsync(int jobId, string employerId)
        {
            var sql = @"
            SELECT
                ai.id            AS Id,
                ai.application_id AS ApplicationId,
                ai.job_id        AS JobId,
                ai.employer_id   AS EmployerId,
                ai.user_id       AS UserId,
                up.name          AS CandidateName,
                up.email         AS CandidateEmail,
                cp.company_name  AS CompanyName,
                ai.room_id       AS RoomId,
                ai.notes         AS Notes,
                ai.scheduled_at  AS ScheduledAt,
                ai.status        AS Status,
                ai.created_at    AS CreatedAt
            FROM application_interviews ai
            LEFT JOIN user_profiles up ON up.user_id = ai.user_id
            LEFT JOIN company_profiles cp ON cp.user_id = ai.employer_id
            WHERE ai.job_id = @JobId
              AND ai.employer_id = @EmployerId
            ORDER BY ai.scheduled_at DESC;";

            return await _db.QueryAsync<VideoInterviewDto>(sql, new
            {
                JobId = jobId,
                EmployerId = employerId
            });
        }

        public async Task<IEnumerable<VideoInterviewDto>> GetMyInterviewsAsync(string userId)
        {
            var sql = @"
            SELECT
                ai.id            AS Id,
                ai.application_id AS ApplicationId,
                ai.job_id        AS JobId,
                ai.employer_id   AS EmployerId,
                ai.user_id       AS UserId,
                up.name          AS CandidateName,
                up.email         AS CandidateEmail,
                cp.company_name  AS CompanyName,
                ai.room_id       AS RoomId,
                ai.notes         AS Notes,
                ai.scheduled_at  AS ScheduledAt,
                ai.status        AS Status,
                ai.created_at    AS CreatedAt
            FROM application_interviews ai
            LEFT JOIN user_profiles up ON up.user_id = ai.user_id
            LEFT JOIN company_profiles cp ON cp.user_id = ai.employer_id
            WHERE ai.user_id = @UserId
            ORDER BY ai.scheduled_at DESC;";

            return await _db.QueryAsync<VideoInterviewDto>(sql, new { UserId = userId });
        }

        public async Task<VideoInterviewDto?> GetInterviewByIdAsync(int interviewId, string userId)
        {
            var sql = @"
            SELECT
                ai.id             AS Id,
                ai.application_id AS ApplicationId,
                ai.job_id         AS JobId,
                ai.employer_id    AS EmployerId,
                ai.user_id        AS UserId,
                up.name           AS CandidateName,
                up.email          AS CandidateEmail,
                cp.company_name   AS CompanyName,
                ai.room_id        AS RoomId,
                ai.notes          AS Notes,
                ai.scheduled_at   AS ScheduledAt,
                ai.status         AS Status,
                ai.created_at     AS CreatedAt
            FROM application_interviews ai
            LEFT JOIN user_profiles up ON up.user_id = ai.user_id
            LEFT JOIN company_profiles cp ON cp.user_id = ai.employer_id
            WHERE ai.id = @InterviewId
              AND (ai.user_id = @UserId OR ai.employer_id = @UserId)
            LIMIT 1;";

            return await _db.QueryFirstOrDefaultAsync<VideoInterviewDto>(sql, new
            {
                InterviewId = interviewId,
                UserId = userId
            });
        }

        public async Task<bool> CanAccessInterviewAsync(int interviewId, string userId)
        {
            var sql = @"
            SELECT COUNT(1)
            FROM application_interviews
            WHERE id = @InterviewId
              AND (user_id = @UserId OR employer_id = @UserId);";

            var count = await _db.ExecuteScalarAsync<int>(sql, new
            {
                InterviewId = interviewId,
                UserId = userId
            });

            return count > 0;
        }

        public async Task<IEnumerable<ApplicationChatRoomDto>> GetChatRoomsAsync(string userId)
        {
            var sql = @"
            SELECT
                ja.id AS ApplicationId,
                ja.job_id AS JobId,
                COALESCE(j.title, '') AS JobTitle,
                COALESCE(ja.status, '') AS Status,
                CASE
                    WHEN j.employerid = @UserId THEN ja.user_id
                    ELSE j.employerid
                END AS ParticipantId,
                CASE
                    WHEN j.employerid = @UserId THEN COALESCE(candidate.name, 'Candidate')
                    ELSE COALESCE(cp.company_name, COALESCE(employer_profile.name, 'Employer'))
                END AS ParticipantName,
                CASE
                    WHEN j.employerid = @UserId THEN candidate.avatar
                    ELSE cp.logo_url
                END AS ParticipantAvatar,
                last_msg.message AS LastMessage,
                last_msg.created_at AS LastMessageAt
            FROM job_applications ja
            INNER JOIN jobs j ON j.id = ja.job_id
            LEFT JOIN user_profiles candidate ON candidate.user_id = ja.user_id
            LEFT JOIN user_profiles employer_profile ON employer_profile.user_id = j.employerid
            LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
            LEFT JOIN LATERAL (
                SELECT acm.message, acm.created_at
                FROM application_chat_messages acm
                WHERE acm.application_id = ja.id
                ORDER BY acm.created_at DESC
                LIMIT 1
            ) last_msg ON TRUE
            WHERE (ja.user_id = @UserId OR j.employerid = @UserId)
              AND ja.status IN ('Shortlisted', 'Hired')
            ORDER BY COALESCE(last_msg.created_at, ja.applied_at) DESC;";

            return await _db.QueryAsync<ApplicationChatRoomDto>(sql, new { UserId = userId });
        }

        public async Task<IEnumerable<ApplicationChatMessageDto>> GetChatMessagesAsync(int applicationId, string userId)
        {
            var canAccess = await CanAccessApplicationChatAsync(applicationId, userId);
            if (!canAccess) return [];

            var sql = @"
            SELECT
                acm.id AS Id,
                acm.application_id AS ApplicationId,
                acm.sender_id AS SenderId,
                COALESCE(up.name, au.""UserName"", 'User') AS SenderName,
                acm.message AS Message,
                acm.created_at AS CreatedAt
            FROM application_chat_messages acm
            LEFT JOIN user_profiles up ON up.user_id = acm.sender_id
            LEFT JOIN ""AspNetUsers"" au ON au.""Id"" = acm.sender_id
            WHERE acm.application_id = @ApplicationId
            ORDER BY acm.created_at ASC;";

            return await _db.QueryAsync<ApplicationChatMessageDto>(sql, new { ApplicationId = applicationId });
        }

        public async Task<ApplicationChatMessageDto?> AddChatMessageAsync(int applicationId, string senderId, string message)
        {
            var canAccess = await CanAccessApplicationChatAsync(applicationId, senderId);
            if (!canAccess) return null;

            var sql = @"
            INSERT INTO application_chat_messages (application_id, sender_id, message, created_at)
            VALUES (@ApplicationId, @SenderId, @Message, CURRENT_TIMESTAMP)
            RETURNING
                id AS Id,
                application_id AS ApplicationId,
                sender_id AS SenderId,
                COALESCE((
                    SELECT up.name FROM user_profiles up WHERE up.user_id = sender_id LIMIT 1
                ), (
                    SELECT au.""UserName"" FROM ""AspNetUsers"" au WHERE au.""Id"" = sender_id LIMIT 1
                ), 'User') AS SenderName,
                message AS Message,
                created_at AS CreatedAt;";

            return await _db.QueryFirstOrDefaultAsync<ApplicationChatMessageDto>(sql, new
            {
                ApplicationId = applicationId,
                SenderId = senderId,
                Message = message.Trim()
            });
        }

        public async Task<bool> CanAccessApplicationChatAsync(int applicationId, string userId)
        {
            var sql = @"
            SELECT COUNT(1)
            FROM job_applications ja
            INNER JOIN jobs j ON j.id = ja.job_id
            WHERE ja.id = @ApplicationId
              AND ja.status IN ('Shortlisted', 'Hired')
              AND (ja.user_id = @UserId OR j.employerid = @UserId);";

            var count = await _db.ExecuteScalarAsync<int>(sql, new
            {
                ApplicationId = applicationId,
                UserId = userId
            });

            return count > 0;
        }

        public async Task<IEnumerable<PostedJobDto>> GetPublicJobsAsync(string? search, string? location, int? jobTypeId)
        {
            var sql = @"
            SELECT
                j.id, j.title, j.description, j.location,
                j.job_type_id   AS JobTypeId,
                jt.name         AS JobType,
                j.salarymin     AS SalaryMin,
                j.salarymax     AS SalaryMax,
                j.status,
                j.createdat     AS CreatedAt,
                COALESCE(j.views, 0) AS Views,
                COALESCE((SELECT COUNT(*) FROM job_applications ja WHERE ja.job_id = j.id), 0) AS Applicants,
                cp.id           AS CompanyId,
                cp.company_name AS CompanyName,
                cp.logo_url     AS CompanyLogo,
                cp.description  AS CompanyDescription
            FROM jobs j
            LEFT JOIN job_types jt ON jt.id = j.job_type_id
            LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
            WHERE j.status = 'Active'
              AND (@Search   IS NULL OR j.title    ILIKE '%' || @Search   || '%')
              AND (@Location IS NULL OR j.location ILIKE '%' || @Location || '%')
              AND (@JobTypeId IS NULL OR j.job_type_id = @JobTypeId)
            ORDER BY j.createdat DESC;";

            var jobs = (await _db.QueryAsync<PostedJobDto>(sql, new { Search = search, Location = location, JobTypeId = jobTypeId })).ToList();

            foreach (var job in jobs)
            {
                job.Skills = (await _db.QueryAsync<SkillTagDto>(@"
                    SELECT s.id, s.name FROM job_skills js
                    JOIN skills s ON js.skill_id = s.id
                    WHERE js.job_id = @JobId", new { JobId = job.Id })).ToList();
            }

            return jobs;
        }

        public async Task<PostedJobDto?> GetPublicJobByIdAsync(int jobId)
        {
            var sql = @"
            SELECT
                j.id, j.title, j.description, j.location,
                j.job_type_id   AS JobTypeId,
                jt.name         AS JobType,
                j.salarymin     AS SalaryMin,
                j.salarymax     AS SalaryMax,
                j.status,
                j.createdat     AS CreatedAt,
                COALESCE(j.views, 0) AS Views,
                COALESCE((SELECT COUNT(*) FROM job_applications ja WHERE ja.job_id = j.id), 0) AS Applicants,
                cp.id           AS CompanyId,
                cp.company_name AS CompanyName,
                cp.logo_url     AS CompanyLogo,
                cp.description  AS CompanyDescription
            FROM jobs j
            LEFT JOIN job_types jt ON jt.id = j.job_type_id
            LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
            WHERE j.id = @JobId;";

            var job = await _db.QueryFirstOrDefaultAsync<PostedJobDto>(sql, new { JobId = jobId });
            if (job == null) return null;

            job.Skills = (await _db.QueryAsync<SkillTagDto>(@"
                SELECT s.id, s.name FROM job_skills js
                JOIN skills s ON js.skill_id = s.id
                WHERE js.job_id = @JobId", new { JobId = jobId })).ToList();

            return job;
        }

        public async Task<IEnumerable<PostedJobDto>> GetRecommendedJobsAsync(string userId)
        {
            // Jobs that share at least one skill with the user's skills, ranked by match count
            var sql = @"
            SELECT
                j.id, j.title, j.description, j.location,
                j.job_type_id   AS JobTypeId,
                jt.name         AS JobType,
                j.salarymin     AS SalaryMin,
                j.salarymax     AS SalaryMax,
                j.status,
                j.createdat     AS CreatedAt,
                COALESCE(j.views, 0) AS Views,
                COALESCE((SELECT COUNT(*) FROM job_applications ja WHERE ja.job_id = j.id), 0) AS Applicants,
                cp.id           AS CompanyId,
                cp.company_name AS CompanyName,
                cp.logo_url     AS CompanyLogo,
                cp.description  AS CompanyDescription,
                COUNT(js.skill_id) AS MatchCount
            FROM jobs j
            LEFT JOIN job_types jt ON jt.id = j.job_type_id
            LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
            INNER JOIN job_skills js ON js.job_id = j.id
            INNER JOIN user_skills us ON us.skill_id = js.skill_id AND us.user_id = @UserId
            WHERE j.status = 'Active'
            GROUP BY j.id, jt.name, cp.id, cp.company_name, cp.logo_url, cp.description
            ORDER BY MatchCount DESC, j.createdat DESC
            LIMIT 20;";

            var jobs = (await _db.QueryAsync<PostedJobDto>(sql, new { UserId = userId })).ToList();

            foreach (var job in jobs)
            {
                job.Skills = (await _db.QueryAsync<SkillTagDto>(@"
                    SELECT s.id, s.name FROM job_skills js
                    JOIN skills s ON js.skill_id = s.id
                    WHERE js.job_id = @JobId", new { JobId = job.Id })).ToList();
            }

            return jobs;
        }
    }
}
