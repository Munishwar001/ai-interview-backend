using AIInterview.Infrastructure.Extensions;
using AIInterview.Application.Extensions;

namespace AIInterview.Server.Extensions
{
    public static class ServiceRegisterationExtension
    {
        public static IServiceCollection RegisterServices(this IServiceCollection services, string connectionString)
        {
            services.RegisterApplicationServices();
            services.RegisterInfrastructureServices(connectionString);
            return services;
        }
    }
}
