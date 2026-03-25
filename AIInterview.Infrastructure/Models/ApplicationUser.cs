using Microsoft.AspNetCore.Identity;

namespace AIInterview.Infrastructure.Models
{
    public class ApplicationUser : IdentityUser
    {
        public string FullName { get; set; }
        public bool IsActive { get; set; } = true;
    }
}
