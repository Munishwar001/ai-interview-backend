using global::AIInterview.Core.Constants;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.DependencyInjection;

namespace AIInterview.Infrastructure.Seed
{
        public static class RoleSeeder
        {
            public static async Task SeedRolesAsync(IServiceProvider serviceProvider)
            {
                
                using var scope = serviceProvider.CreateScope();
                var roleManager = serviceProvider.GetRequiredService<RoleManager<IdentityRole>>();

                string[] roles =
                {
                    AppRoles.JobSeeker,
                    AppRoles.Employer,
                };

                foreach (var role in roles)
                {
                    if (!await roleManager.RoleExistsAsync(role))
                    {
                        await roleManager.CreateAsync(new IdentityRole(role));
                    }
                }
            }
        }
    }
