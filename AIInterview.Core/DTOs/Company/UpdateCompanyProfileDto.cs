namespace AIInterview.Core.DTOs.Company
{

    public class CompanyProfile
    {
        public int Id { get; set; }

        public string UserId { get; set; } = string.Empty;

        public string CompanyName { get; set; } = string.Empty;
        public string? Tagline { get; set; }
        public string? Description { get; set; }

        public string? Website { get; set; }
        public string? Industry { get; set; }

        public int? CompanySizeId { get; set; }
        public int? FoundedYear { get; set; }

        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }

        public bool IsVerified { get; set; }
        public int ProfileCompletionPercentage { get; set; }
        public int ProfileViews { get; set; }

        public DateTime CreatedAt { get; set; }
        public DateTime? UpdatedAt { get; set; }
    }

    public class UpdateCompanyProfileDto
    {
        public string? UserId { get; set; }

        public string CompanyName { get; set; } = string.Empty;
        public string? Tagline { get; set; }
        public string? Description { get; set; }

        public string? Website { get; set; }
        public string? Industry { get; set; }

        public int? CompanySizeId { get; set; }
        public int? FoundedYear { get; set; }

        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }

        public string? Email { get; set; }
        public string? Phone { get; set; }

        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }

        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }
    }

    public class GenerateCompanyDescriptionDto
    {
        public string CompanyName { get; set; } = string.Empty;
        public string? Industry { get; set; }
        public string? Tagline { get; set; }
    }

    public class CompanyProfileResponseDto
    {
        public int Id { get; set; }
        public string CompanyName { get; set; } = string.Empty;
        public string? Tagline { get; set; }
        public string? Description { get; set; }
        public string? Website { get; set; }
        public string? Industry { get; set; }
        public int? CompanySizeId { get; set; }
        public string? CompanySizeLabel { get; set; }
        public int? FoundedYear { get; set; }
        public string? LogoUrl { get; set; }
        public string? CoverImageUrl { get; set; }
        public string? Email { get; set; }
        public string? Phone { get; set; }
        public string? AddressLine1 { get; set; }
        public string? AddressLine2 { get; set; }
        public string? City { get; set; }
        public string? State { get; set; }
        public string? Country { get; set; }
        public string? PostalCode { get; set; }
        public string? LinkedInUrl { get; set; }
        public string? TwitterUrl { get; set; }
        public bool IsVerified { get; set; }
        public int ProfileCompletionPercentage { get; set; }
        public int ProfileViews { get; set; }
    }

}
