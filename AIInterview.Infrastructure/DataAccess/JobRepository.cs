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

        public async Task<int> CreateJobAsync(CreateJobDto request)
        {
            var sql = @"
            INSERT INTO jobs (title, description, location, salarymin, salarymax, companyname, employerid)
            VALUES (@Title, @Description, @Location, @SalaryMin, @SalaryMax, @CompanyName, @EmployerId)
            RETURNING id;
        ";

            var jobId = await _db.ExecuteScalarAsync<int>(sql, request);

            if (request.SkillIds != null && request.SkillIds.Any())
            {
                foreach (var skillId in request.SkillIds)
                {
                    await _db.ExecuteAsync(@"
                    INSERT INTO job_skills (job_id, skill_id)
                    VALUES (@JobId, @SkillId)",
                        new { JobId = jobId, SkillId = skillId });
                }
            }

            return jobId;
        }

        public async Task<IEnumerable<object>> GetAllJobsAsync()
        {
            return await _db.QueryAsync("SELECT * FROM jobs ORDER BY createdat DESC");
        }

        public async Task<object> GetJobByIdAsync(int id)
        {
            var job = await _db.QueryFirstOrDefaultAsync("SELECT * FROM jobs WHERE id = @Id", new { Id = id });

            if (job == null) return null;

            var skills = await _db.QueryAsync(@"
            SELECT s.id, s.name FROM job_skills js JOIN skills s ON js.skill_id = s.id WHERE js.job_id = @JobId",
                new { JobId = id });

            return new { job, skills };
        }

        public async Task<IEnumerable<object>> GetJobsByEmployerAsync(string employerId)
        {
            return await _db.QueryAsync(@"
            SELECT * FROM jobs WHERE employerid = @EmployerId ORDER BY createdat DESC",
                new { EmployerId = employerId });
        }
    }
}
