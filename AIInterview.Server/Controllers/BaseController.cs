using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using System.Security.Claims;

namespace AIInterview.Server.Controllers
{
    [ApiController]
    public class BaseController : ControllerBase
    {
        protected string CurrentUserID => User.FindFirst(ClaimTypes.NameIdentifier)?.Value;
        protected string CurrentUserEmail => User.FindFirst(ClaimTypes.Email)?.Value;

        protected IActionResult CustomUnauthorized401(string? message = null, string? errorCategory = null)
        {
            var problemDetailsFactory = HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            ProblemDetails problemDetails;

            string detailMessage = string.IsNullOrWhiteSpace(message)
                ? "Unauthorized Access."
                : message;

            problemDetails = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status401Unauthorized,
                detail: detailMessage
            );

            if (!string.IsNullOrWhiteSpace(errorCategory))
            {
                problemDetails.Extensions["errorCategory"] = errorCategory;
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };
        }

        protected IActionResult CustomProblem400(string? error = null)
        {
            if (string.IsNullOrWhiteSpace(error))
            {
                return CustomProblem400([]);
            }
            return CustomProblem400([error]);
        }

        protected IActionResult CustomProblem400(IEnumerable<string> errors)
        {
            var problemDetailsFactory = HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();
            ProblemDetails problemDetails;

            if (!errors.Any())
            {
                problemDetails = problemDetailsFactory.CreateProblemDetails(
                  HttpContext,
                  statusCode: StatusCodes.Status400BadRequest,
                  detail: "An error occurred while processing your request!"
              );
            }
            else if (errors.Count() > 1)
            {
                problemDetails = problemDetailsFactory.CreateProblemDetails(
                    HttpContext,
                    statusCode: StatusCodes.Status400BadRequest,
                    detail: "One or more validation errors occurred."
                );
                problemDetails.Extensions["errorMessages"] = errors;
            }
            else
            {
                problemDetails = problemDetailsFactory.CreateProblemDetails(
                   HttpContext,
                   statusCode: StatusCodes.Status400BadRequest,
                   detail: errors.First()
               );
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };
        }
        protected IActionResult CustomProblem500(string? message = null, Exception? exception = null)
        {
            var problemDetailsFactory =
                HttpContext.RequestServices.GetRequiredService<ProblemDetailsFactory>();

            var detailMessage = string.IsNullOrWhiteSpace(message)
                ? "An unexpected error occurred. Please try again later."
                : message;

            var problemDetails = problemDetailsFactory.CreateProblemDetails(
                HttpContext,
                statusCode: StatusCodes.Status500InternalServerError,
                detail: detailMessage
            );

            if (exception != null)
            {
                problemDetails.Extensions["errorMessage"] = exception.Message;

                var env = HttpContext.RequestServices.GetRequiredService<IWebHostEnvironment>();
                if (env.IsDevelopment())
                    problemDetails.Extensions["stackTrace"] = exception.StackTrace;
            }

            return new ObjectResult(problemDetails)
            {
                StatusCode = problemDetails.Status
            };
        }
    }
}
