using AIInterview.Application.Interface;
using AIInterview.Core.DTOs.Company;

namespace AIInterview.Application.Services
{
    public class CompanyService
    {
        private readonly ICompanyRepository _repo;

        public CompanyService(ICompanyRepository repo)
        {
            _repo = repo;
        }

        public async Task<object> UpsertAsync(UpdateCompanyProfileDto dto)
        {
            try
            {
                var existing = await _repo.GetByUserIdAsync(dto.UserId!);

                if (existing == null)
                {
                    var profile = MapToProfile(dto);
                    var id = await _repo.InsertAsync(profile);
                    profile.Id = id;
                    profile.ProfileCompletionPercentage = CalculateCompletion(profile);
                    return profile;
                }
                else
                {
                    MapToExisting(existing, dto);
                    existing.ProfileCompletionPercentage = CalculateCompletion(existing);
                    await _repo.UpdateAsync(existing);
                    return existing;
                }
            }
            catch (Exception) { throw; }
        }

        public async Task<CompanyProfileResponseDto?> GetByUserIdAsync(string userId)
        {
            try
            {
                return await _repo.GetProfileWithSizeAsync(userId);
            }
            catch (Exception) { throw; }
        }

        public async Task<object> UpdateImagesAsync(string userId, string? logoUrl, string? coverImageUrl)
        {
            try
            {
                await _repo.UpdateImageAsync(userId, logoUrl, coverImageUrl);
                return await _repo.GetProfileWithSizeAsync(userId);
            }
            catch (Exception) { throw; }
        }

        // Maps DTO to a new CompanyProfile entity
        private static CompanyProfile MapToProfile(UpdateCompanyProfileDto dto) => new()
        {
            UserId = dto.UserId!,
            CompanyName = dto.CompanyName,
            Tagline = dto.Tagline,
            Description = dto.Description,
            Website = dto.Website,
            Industry = dto.Industry,
            CompanySizeId = dto.CompanySizeId,
            FoundedYear = dto.FoundedYear,
            LogoUrl = dto.LogoUrl,
            CoverImageUrl = dto.CoverImageUrl,
            Email = dto.Email,
            Phone = dto.Phone,
            AddressLine1 = dto.AddressLine1,
            AddressLine2 = dto.AddressLine2,
            City = dto.City,
            State = dto.State,
            Country = dto.Country,
            PostalCode = dto.PostalCode,
            LinkedInUrl = dto.LinkedInUrl,
            TwitterUrl = dto.TwitterUrl,
        };

        // Maps DTO fields onto an existing profile
        private static void MapToExisting(CompanyProfile existing, UpdateCompanyProfileDto dto)
        {
            existing.CompanyName = dto.CompanyName;
            existing.Tagline = dto.Tagline;
            existing.Description = dto.Description;
            existing.Website = dto.Website;
            existing.Industry = dto.Industry;
            existing.CompanySizeId = dto.CompanySizeId;
            existing.FoundedYear = dto.FoundedYear;
            existing.LogoUrl = dto.LogoUrl ?? existing.LogoUrl;
            existing.CoverImageUrl = dto.CoverImageUrl ?? existing.CoverImageUrl;
            existing.Email = dto.Email;
            existing.Phone = dto.Phone;
            existing.AddressLine1 = dto.AddressLine1;
            existing.AddressLine2 = dto.AddressLine2;
            existing.City = dto.City;
            existing.State = dto.State;
            existing.Country = dto.Country;
            existing.PostalCode = dto.PostalCode;
            existing.LinkedInUrl = dto.LinkedInUrl;
            existing.TwitterUrl = dto.TwitterUrl;
        }

        private static int CalculateCompletion(CompanyProfile dto)
        {
            var fields = new[]
            {
                !string.IsNullOrWhiteSpace(dto.CompanyName),
                !string.IsNullOrWhiteSpace(dto.Description),
                !string.IsNullOrWhiteSpace(dto.Website),
                !string.IsNullOrWhiteSpace(dto.Industry),
                dto.CompanySizeId.HasValue,
                !string.IsNullOrWhiteSpace(dto.City),
                !string.IsNullOrWhiteSpace(dto.Country),
                !string.IsNullOrWhiteSpace(dto.Phone),
                !string.IsNullOrWhiteSpace(dto.LogoUrl),
                !string.IsNullOrWhiteSpace(dto.CoverImageUrl),
                !string.IsNullOrWhiteSpace(dto.AddressLine1),
                !string.IsNullOrWhiteSpace(dto.AddressLine2),
                dto.FoundedYear.HasValue && dto.FoundedYear > 0,
                !string.IsNullOrWhiteSpace(dto.LinkedInUrl)
            };

            int filled = fields.Count(f => f);
            return (int)Math.Round((double)filled / fields.Length * 100);
        }
    }
}
