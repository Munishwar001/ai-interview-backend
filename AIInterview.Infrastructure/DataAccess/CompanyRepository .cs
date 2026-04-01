using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Company;
using Dapper;
using System.Data;

namespace AIInterview.Infrastructure.DataAccess
{   

    public class CompanyRepository : ICompanyRepository
    {
        private readonly IDbConnection _db;
        public CompanyRepository(IDbConnection db)
        {
            _db = db;
        }

        public async Task<CompanyProfile?> GetByUserIdAsync(string userId)
        {
            var sql = @"SELECT * FROM company_profiles WHERE user_id = @UserId LIMIT 1";
            return await _db.QueryFirstOrDefaultAsync<CompanyProfile>(sql, new { UserId = userId });
        }

        public async Task<int> InsertAsync(CompanyProfile profile)
        {
            var sql = @"
            INSERT INTO company_profiles
            (user_id, company_name, website, company_size_id, description, industry, city, country, created_at)
            VALUES
            (@UserId, @CompanyName, @Website, @CompanySizeId, @Description, @Industry, @City, @Country, CURRENT_TIMESTAMP)
            RETURNING id;
        ";

            return await _db.ExecuteScalarAsync<int>(sql, profile);
        }

        public async Task UpdateAsync(CompanyProfile profile)
        {
            var sql = @"
            UPDATE company_profiles SET
                company_name = @CompanyName,
                website = @Website,
                company_size_id = @CompanySizeId,
                description = @Description,
                industry = @Industry,
                city = @City,
                country = @Country,
                updated_at = CURRENT_TIMESTAMP
            WHERE user_id = @UserId;
        ";

            await _db.ExecuteAsync(sql, profile);
        }

        public async Task<CompanyProfileResponseDto?> GetProfileWithSizeAsync(string userId)
        {
            var sql = @"
            SELECT 
                cp.id,
                cp.company_name AS CompanyName,
                cp.tagline,
                cp.description,
                cp.website,
                cp.industry,
                cp.company_size_id AS CompanySizeId,
                cs.label AS CompanySizeLabel,
                cp.logo_url AS LogoUrl,
                cp.city,
                cp.country,
                cp.profile_completion_percentage AS ProfileCompletionPercentage
            FROM company_profiles cp
            LEFT JOIN company_sizes cs 
                ON cp.company_size_id = cs.id
            WHERE cp.user_id = @UserId
            LIMIT 1;
        ";

            return await _db.QueryFirstOrDefaultAsync<CompanyProfileResponseDto>(
                sql,
                new { UserId = userId }
            );
        }
    }
}
