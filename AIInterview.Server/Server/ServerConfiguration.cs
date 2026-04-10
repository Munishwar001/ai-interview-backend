using AIInterview.Application.Interface;
using AIInterview.Application.Services;
using AIInterview.Core.Comman;
using AIInterview.Infrastructure.Data;
using AIInterview.Infrastructure.Seed;
using AIInterview.Server.Extensions;
using AIInterview.Server.Hubs;
using AIInterview.Server.Middlewares;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace AIInterview.Server
{
    public static class ServerConfiguration
    {
        public static void ConfigureServices(WebApplicationBuilder builder)
        {
            var services = builder.Services;
            var config = builder.Configuration;
            var connectionString = config.GetConnectionString("Default");

            services.AddControllers();
            services.AddSignalR();

            services.ConfigureIdentity();

            services.AddJwtBearerAuthentication(config);

            services.AddCorsPolicy(config);
            services.ConfigureApiBehavior();

            services.AddEndpointsApiExplorer();
            services.AddSwaggerGen();

            services.AddDbContext<ApplicationDbContext>(options =>
                options.UseNpgsql(connectionString));

            services.Configure<JwtConfig>(options => config.GetSection("Jwt").Bind(options));
            
            services.RegisterServices(connectionString);

            services.AddHttpClient<GroqAiService>();
            services.AddHttpClient<OllamaAiService>();
            services.AddScoped<IAiService>(sp =>
            {
                var primary  = sp.GetRequiredService<GroqAiService>();
                var fallback = sp.GetRequiredService<OllamaAiService>();
                var logger   = sp.GetRequiredService<ILogger<FallbackAiService>>();
                return new FallbackAiService(primary, fallback, logger);
            });

        }

        public static async Task ConfigurePipeline(WebApplication app)
        {
            if (app.Environment.IsDevelopment())
            {
                app.UseSwagger();
                app.UseSwaggerUI();
            }

            app.UseHttpsRedirection();

            app.UseStaticFiles();

            app.UseCors("AllowFrontend");

            app.UseAuthentication();
            app.DecryptClaims();
            app.UseAuthorization();

            app.MapControllers();
            app.MapHub<InterviewHub>("/hubs/interview");
            app.MapHub<ApplicationChatHub>("/hubs/application-chat");

            using var scope = app.Services.CreateScope();
            var services = scope.ServiceProvider;
            await RoleSeeder.SeedRolesAsync(services);
        }
    }
}