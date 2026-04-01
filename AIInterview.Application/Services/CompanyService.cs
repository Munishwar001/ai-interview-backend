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
            var existing = await _repo.GetByUserIdAsync(dto.UserId!);

            if (existing == null)
            {
                // INSERT
                var profile = new CompanyProfile
                {
                    UserId = dto.UserId!,
                    CompanyName = dto.CompanyName,
                    Website = dto.Website,
                    CompanySizeId = dto.CompanySizeId,
                    Description = dto.Description,
                    Industry = dto.Industry,
                    City = dto.City,
                    Country = dto.Country
                };

                var id = await _repo.InsertAsync(profile);
                profile.Id = id;

                return profile;
            }
            else
            {
                // UPDATE
                existing.CompanyName = dto.CompanyName;
                existing.Website = dto.Website;
                existing.CompanySizeId = dto.CompanySizeId;
                existing.Description = dto.Description;
                existing.Industry = dto.Industry;
                existing.City = dto.City;
                existing.Country = dto.Country;

                await _repo.UpdateAsync(existing);

                return existing;
            }
        }
        public async Task<CompanyProfileResponseDto?> GetByUserIdAsync(string userId)
        {
            var profile = await _repo.GetProfileWithSizeAsync(userId);

            if (profile == null) return null;

            return profile;
        }

    }
}
