using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;

namespace AIInterview.Server.Extensions
{
    public static class ApiBehaviorExtension
    {
        public static void ConfigureApiBehavior(this IServiceCollection services)
        {
            services.Configure<ApiBehaviorOptions>(options =>
            {
                options.InvalidModelStateResponseFactory = context =>
                {
                    var errors = context.ModelState
                        .Where(x => x.Value?.Errors.Count > 0)
                        .SelectMany(x => x.Value!.Errors)
                        .Select(x => x.ErrorMessage)
                        .ToList();

                    var problemDetailsFactory = context.HttpContext.RequestServices
                        .GetRequiredService<ProblemDetailsFactory>();

                    var problemDetails = problemDetailsFactory.CreateProblemDetails(
                        context.HttpContext,
                        statusCode: StatusCodes.Status400BadRequest,
                        detail: "One or more validation errors occurred."
                    );

                    problemDetails.Extensions["errorMessages"] = errors;

                    return new BadRequestObjectResult(problemDetails);
                };
            });
        }
    }
}