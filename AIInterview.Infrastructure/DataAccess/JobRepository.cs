using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Job;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class JobRepository : IJobRepository
    {
        private readonly IDbConnection _db;

        public JobRepository(IDbConnection db)
        {
            _db = db;
        }

        #region Write

        public async Task<int> CreateJobAsync(CreateJobDto request)
        {
            try
            {
                var sql = @"
                INSERT INTO jobs (title, description, location, job_type_id, salarymin, salarymax, employerid, status, createdat)
                VALUES (@Title, @Description, @Location, @JobType, @SalaryMin, @SalaryMax, @EmployerId, 'Active', CURRENT_TIMESTAMP)
                RETURNING id;";

                var jobId = await _db.ExecuteScalarAsync<int>(sql, new
                {
                    request.Title, request.Description, request.Location,
                    request.JobType,
                    request.SalaryMin, request.SalaryMax,
                    request.EmployerId
                });

                if (request.SkillIds != null && request.SkillIds.Any())
                {
                    foreach (var skillId in request.SkillIds)
                    {
                        await _db.ExecuteAsync(
                            "INSERT INTO job_skills (job_id, skill_id) VALUES (@JobId, @SkillId)",
                            new { JobId = jobId, SkillId = skillId });
                    }
                }

                return jobId;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> UpdateJobAsync(int id, string employerId, UpdateJobDto request)
        {
            try
            {
                var sql = @"
                UPDATE jobs SET
                    title       = @Title,
                    description = @Description,
                    location    = @Location,
                    job_type_id = @JobTypeId,
                    salarymin   = @SalaryMin,
                    salarymax   = @SalaryMax,
                    updated_at  = CURRENT_TIMESTAMP
                WHERE id = @Id AND employerid = @EmployerId;";

                var rows = await _db.ExecuteAsync(sql, new
                {
                    request.Title, request.Description, request.Location,
                    request.JobTypeId, request.SalaryMin, request.SalaryMax,
                    Id = id, EmployerId = employerId
                });

                if (rows > 0 && request.SkillIds != null)
                {
                    await _db.ExecuteAsync("DELETE FROM job_skills WHERE job_id = @JobId", new { JobId = id });
                    foreach (var skillId in request.SkillIds)
                    {
                        await _db.ExecuteAsync(
                            "INSERT INTO job_skills (job_id, skill_id) VALUES (@JobId, @SkillId)",
                            new { JobId = id, SkillId = skillId });
                    }
                }

                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> DeleteJobAsync(int id, string employerId)
        {
            try
            {
                await _db.ExecuteAsync("DELETE FROM job_skills WHERE job_id = @JobId", new { JobId = id });
                var rows = await _db.ExecuteAsync(
                    "DELETE FROM jobs WHERE id = @Id AND employerid = @EmployerId",
                    new { Id = id, EmployerId = employerId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> CloseJobAsync(int id, string employerId)
        {
            try
            {
                var rows = await _db.ExecuteAsync(
                    "UPDATE jobs SET status = 'Closed', updated_at = CURRENT_TIMESTAMP WHERE id = @Id AND employerid = @EmployerId",
                    new { Id = id, EmployerId = employerId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        public async Task<bool> ReopenJobAsync(int id, string employerId)
        {
            try
            {
                var rows = await _db.ExecuteAsync(
                    "UPDATE jobs SET status = 'Active', updated_at = CURRENT_TIMESTAMP WHERE id = @Id AND employerid = @EmployerId",
                    new { Id = id, EmployerId = employerId });
                return rows > 0;
            }
            catch (Exception) { throw; }
        }

        #endregion

        #region Read

        public async Task<IEnumerable<PostedJobDto>> GetMyJobsAsync(string employerId)
        {
            try
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
                    COALESCE((SELECT COUNT(*) FROM job_applications ja WHERE ja.job_id = j.id), 0)                               AS Applicants,
                    COALESCE((SELECT COUNT(*) FROM job_applications ja WHERE ja.job_id = j.id AND ja.status = 'Shortlisted'), 0) AS Shortlisted,
                    cp.id           AS CompanyId,
                    cp.company_name AS CompanyName,
                    cp.logo_url     AS CompanyLogo,
                    cp.description  AS CompanyDescription
                FROM jobs j
                LEFT JOIN job_types jt ON jt.id = j.job_type_id
                LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
                WHERE j.employerid = @EmployerId
                ORDER BY j.createdat DESC;";

                var jobs = (await _db.QueryAsync<PostedJobDto>(sql, new { EmployerId = employerId })).ToList();

                foreach (var job in jobs)
                {
                    job.Skills = (await _db.QueryAsync<SkillTagDto>(@"
                        SELECT s.id, s.name FROM job_skills js
                        JOIN skills s ON js.skill_id = s.id
                        WHERE js.job_id = @JobId", new { JobId = job.Id })).ToList();
                }

                return jobs;
            }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<object>> GetJobApplicantsAsync(int jobId, string employerId)
        {
            try
            {
                var owns = await _db.ExecuteScalarAsync<int>(
                    "SELECT COUNT(1) FROM jobs WHERE id = @JobId AND employerid = @EmployerId",
                    new { JobId = jobId, EmployerId = employerId });

                if (owns == 0) return Enumerable.Empty<object>();

                // AspNetUsers needs quotes — created by EF with mixed case
                return await _db.QueryAsync<object>(@"
                    SELECT
                        ja.id, ja.applied_at AS AppliedAt, ja.status,
                        u.""Id"" AS UserId, up.name, up.email, up.avatar,
                        up.resume_file_path AS ResumeFilePath, up.resume_file_name AS ResumeFileName
                    FROM job_applications ja
                    JOIN ""AspNetUsers"" u ON ja.user_id = u.""Id""
                    LEFT JOIN user_profiles up ON up.user_id = u.""Id""
                    WHERE ja.job_id = @JobId
                    ORDER BY ja.applied_at DESC;",
                    new { JobId = jobId });
            }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<object>> GetAllJobsAsync()
        {
            try { return await _db.QueryAsync("SELECT * FROM jobs ORDER BY createdat DESC"); }
            catch (Exception) { throw; }
        }

        public async Task<object> GetJobByIdAsync(int id)
        {
            try
            {
                var job = await _db.QueryFirstOrDefaultAsync(@"
                    SELECT
                        j.id, j.title, j.description, j.location,
                        j.job_type_id AS JobTypeId,
                        jt.name       AS JobType,
                        j.salarymin   AS SalaryMin,
                        j.salarymax   AS SalaryMax,
                        j.status,
                        j.createdat   AS CreatedAt,
                        cp.id         AS CompanyId,
                        cp.company_name AS CompanyName,
                        cp.logo_url   AS CompanyLogo
                    FROM jobs j
                    LEFT JOIN job_types jt ON jt.id = j.job_type_id
                    LEFT JOIN company_profiles cp ON cp.user_id = j.employerid
                    WHERE j.id = @Id", new { Id = id });

                if (job == null) return null;

                var skills = await _db.QueryAsync<SkillTagDto>(@"
                    SELECT s.id, s.name FROM job_skills js
                    JOIN skills s ON js.skill_id = s.id
                    WHERE js.job_id = @JobId", new { JobId = id });

                return new { job, skills };
            }
            catch (Exception) { throw; }
        }

        public async Task<IEnumerable<object>> GetJobsByEmployerAsync(string employerId)
        {
            try
            {
                return await _db.QueryAsync(
                    "SELECT * FROM jobs WHERE employerid = @EmployerId ORDER BY createdat DESC",
                    new { EmployerId = employerId });
            }
            catch (Exception) { throw; }
        }

        #endregion
    }
}
