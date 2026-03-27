using AIInterview.Application.Interface;
using AIInterview.Core.Comman;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{
    public class LookupRepository : ILookupRepository
    {
        private readonly IDbConnection _db;

        public LookupRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<IEnumerable<dynamic>> GetJobTypesAsync()
        {
            var query = @"SELECT id, name FROM job_types ORDER BY id";
            return await _db.QueryAsync(query);
        }

        public async Task<IEnumerable<LookupDto>> GetSkillsAsync()
        {
            var query = "SELECT Id, Name FROM Skills";
            return await _db.QueryAsync<LookupDto>(query);
        }

    }
}
