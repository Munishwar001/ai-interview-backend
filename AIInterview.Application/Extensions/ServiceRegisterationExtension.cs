using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using Microsoft.Extensions.DependencyInjection;

namespace AIInterview.Application.Extensions
{
    public static class ServiceRegisterationExtension
    {
        public static IServiceCollection RegisterApplicationServices(this IServiceCollection services)
        {
            services.AddScoped<JwtAuthManager>();
            services.AddScoped<UserService>();
            services.AddScoped<JobService>();
            services.AddScoped<LookupService>();
            services.AddScoped<CompanyService>();
            services.AddScoped<IEncryptionService ,EncryptionService>();

            return services;
        }
    }
}
