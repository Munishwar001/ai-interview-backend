using AIInterview.Application.Interface;
using AIInterview.Infrastructure.DataAccess;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;
using System.Data;

namespace AIInterview.Infrastructure.Extensions
{
    public static class ServiceRegisterationExtension
    {
        public static IServiceCollection RegisterInfrastructureServices(this IServiceCollection services, string connectionString)
        {
            services.AddScoped<IDbConnection>(_ => new NpgsqlConnection(connectionString));
            services.AddScoped<IUserRepository ,UserRepository>();
            services.AddScoped<IJobRepository, JobRepository>();
            services.AddScoped<ILookupRepository, LookupRepository>();
            services.AddScoped<ICompanyRepository, CompanyRepository>();
            services.AddScoped<IUserProfileRepository, UserProfileRepository>();
            services.AddScoped<IUserExperienceRepository, UserExperienceRepository>();
            services.AddScoped<IUserEducationRepository, UserEducationRepository>();
            services.AddScoped<IUserSkillRepository, UserSkillRepository>();
            services.AddScoped<IResumeAnalysisRepository, ResumeAnalysisRepository>();
            services.AddScoped<IMockInterviewRepository, MockInterviewRepository>();

            return services;
        }
    }
}
