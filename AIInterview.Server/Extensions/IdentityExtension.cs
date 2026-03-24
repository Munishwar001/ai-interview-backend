using AIInterview.Infrastructure.Data;
using AIInterview.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace AIInterview.Server.Extensions
{
    public static class IdentityExtension
    {
        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
