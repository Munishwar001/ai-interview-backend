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
