using AIInterview.Application.Extensions;
using AIInterview.Application.Interface;
using AIInterview.Infrastructure.Extensions;
using AIInterview.Server.Services;

namespace AIInterview.Server.Extensions
{
    public static class ServiceRegisterationExtension
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, string connectionString)
        {
            services.AddSingleton<ICloudinaryFileService, CloudinaryFileService>();
            services.AddScoped<IEmailService, EmailService>();
            services.RegisterApplicationServices();
            services.RegisterInfrastructureServices(connectionString);
            return services;
        }
    }
}
