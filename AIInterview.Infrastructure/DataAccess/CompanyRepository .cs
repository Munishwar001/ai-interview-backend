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

        #region Read

        public async Task<CompanyProfile?> GetByUserIdAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT
                    id, user_id AS UserId, company_name AS CompanyName, tagline, description,
                    website, industry, company_size_id AS CompanySizeId, founded_year AS FoundedYear,
                    logo_url AS LogoUrl, cover_image_url AS CoverImageUrl, email, phone,
                    address_line1 AS AddressLine1, address_line2 AS AddressLine2,
                    city, state, country, postal_code AS PostalCode,
                    linkedin_url AS LinkedInUrl, twitter_url AS TwitterUrl,
                    is_verified AS IsVerified, profile_completion_percentage AS ProfileCompletionPercentage,
                    created_at AS CreatedAt, updated_at AS UpdatedAt
                FROM company_profiles WHERE user_id = @UserId LIMIT 1";

                return await _db.QueryFirstOrDefaultAsync<CompanyProfile>(sql, new { UserId = userId });
            }
            catch (Exception) { throw; }
        }

        public async Task<CompanyProfileResponseDto?> GetProfileWithSizeAsync(string userId)
        {
            try
            {
                var sql = @"
                SELECT
                    cp.id,
                    cp.company_name          AS CompanyName,
                    cp.tagline,
                    cp.description,
                    cp.website,
                    cp.industry,
                    cp.company_size_id       AS CompanySizeId,
                    cs.label                 AS CompanySizeLabel,
                    cp.founded_year          AS FoundedYear,
                    cp.logo_url              AS LogoUrl,
                    cp.cover_image_url       AS CoverImageUrl,
                    cp.email,
                    cp.phone,
                    cp.address_line1         AS AddressLine1,
                    cp.address_line2         AS AddressLine2,
                    cp.city,
                    cp.state,
                    cp.country,
                    cp.postal_code           AS PostalCode,
                    cp.linkedin_url          AS LinkedInUrl,
                    cp.twitter_url           AS TwitterUrl,
                    cp.is_verified           AS IsVerified,
                    cp.profile_completion_percentage AS ProfileCompletionPercentage
                FROM company_profiles cp
                LEFT JOIN company_sizes cs ON cp.company_size_id = cs.id
                WHERE cp.user_id = @UserId
                LIMIT 1;";

                return await _db.QueryFirstOrDefaultAsync<CompanyProfileResponseDto>(sql, new { UserId = userId });
            }
            catch (Exception) { throw; }
        }

        #endregion

        #region Write

        public async Task<int> InsertAsync(CompanyProfile profile)
        {
            try
            {
                var sql = @"
                INSERT INTO company_profiles
                (user_id, company_name, tagline, website, company_size_id, description, industry,
                 founded_year, logo_url, cover_image_url, email, phone,
                 address_line1, address_line2, city, state, country, postal_code,
                 linkedin_url, twitter_url, profile_completion_percentage, created_at)
                VALUES
                (@UserId, @CompanyName, @Tagline, @Website, @CompanySizeId, @Description, @Industry,
                 @FoundedYear, @LogoUrl, @CoverImageUrl, @Email, @Phone,
                 @AddressLine1, @AddressLine2, @City, @State, @Country, @PostalCode,
                 @LinkedInUrl, @TwitterUrl, @ProfileCompletionPercentage, CURRENT_TIMESTAMP)
                RETURNING id;";

                return await _db.ExecuteScalarAsync<int>(sql, profile);
            }
            catch (Exception) { throw; }
        }

        public async Task UpdateAsync(CompanyProfile profile)
        {
            try
            {
                var sql = @"
                UPDATE company_profiles SET
                    company_name = @CompanyName, tagline = @Tagline, website = @Website,
                    company_size_id = @CompanySizeId, description = @Description, industry = @Industry,
                    founded_year = @FoundedYear, logo_url = @LogoUrl, cover_image_url = @CoverImageUrl,
                    email = @Email, phone = @Phone, address_line1 = @AddressLine1,
                    address_line2 = @AddressLine2, city = @City, state = @State, country = @Country,
                    postal_code = @PostalCode, linkedin_url = @LinkedInUrl, twitter_url = @TwitterUrl,
                    profile_completion_percentage = @ProfileCompletionPercentage,
                    updated_at = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                await _db.ExecuteAsync(sql, profile);
            }
            catch (Exception) { throw; }
        }

        public async Task UpdateImageAsync(string userId, string? logoUrl, string? coverImageUrl)
        {
            try
            {
                var sql = @"
                UPDATE company_profiles SET
                    logo_url = COALESCE(@LogoUrl, logo_url),
                    cover_image_url = COALESCE(@CoverImageUrl, cover_image_url),
                    updated_at = CURRENT_TIMESTAMP
                WHERE user_id = @UserId;";

                await _db.ExecuteAsync(sql, new { UserId = userId, LogoUrl = logoUrl, CoverImageUrl = coverImageUrl });
            }
            catch (Exception) { throw; }
        }

        #endregion
    }
}
