using AIInterview.Infrastructure.Data;
using AIInterview.Infrastructure.Models;
using Microsoft.AspNetCore.Identity;

namespace AIInterview.Server.Extensions
{
    public static class IdentityExtension
    {
        public static IServiceCollection ConfigureIdentity(this IServiceCollection services)
        {
            services.Configure<IdentityOptions>(options =>
            {
                // Password validation - minimum 6 characters only
                options.Password.RequiredLength = 6;
                options.Password.RequireDigit = false;
                options.Password.RequireLowercase = false;
                options.Password.RequireUppercase = false;
                options.Password.RequireNonAlphanumeric = false;
            });

            services.AddIdentity<ApplicationUser, IdentityRole>()
                .AddEntityFrameworkStores<ApplicationDbContext>()
                .AddDefaultTokenProviders();

            return services;
        }
    }
}
