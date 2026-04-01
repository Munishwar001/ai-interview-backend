using AIInterview.Core.DTOs.Company;

namespace AIInterview.Application.Interface
{
    public interface ICompanyRepository
    {
        Task<CompanyProfile?> GetByUserIdAsync(string userId);
        Task<int> InsertAsync(CompanyProfile profile);
        Task UpdateAsync(CompanyProfile profile);
        Task<CompanyProfileResponseDto?> GetProfileWithSizeAsync(string userId);
    }
}