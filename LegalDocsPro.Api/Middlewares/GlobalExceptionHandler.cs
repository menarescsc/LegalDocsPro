using FluentValidation;
using LegalDocsPro.Domain.Exceptions;
using Microsoft.AspNetCore.Diagnostics;
using Microsoft.AspNetCore.Mvc;

namespace LegalDocsPro.Api.Middlewares
{
    /// <summary>
    /// Global exception handler that maps exceptions to appropriate HTTP responses.
    /// </summary>
    public class GlobalExceptionHandler : IExceptionHandler
    {
        private readonly ILogger<GlobalExceptionHandler> _logger;

        public GlobalExceptionHandler(ILogger<GlobalExceptionHandler> logger)
        {
            _logger = logger;
        }

        public async ValueTask<bool> TryHandleAsync(
            HttpContext httpContext,
            Exception exception,
            CancellationToken cancellationToken)
        {
            _logger.LogError(exception, "Unhandled exception caught by GlobalExceptionHandler");

            var problemDetails = exception switch
            {
                ValidationException validationEx => CreateValidationProblemDetails(httpContext, validationEx),
                DomainException domainEx => CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Domain Rule Violation",
                    domainEx.Message),
                UnauthorizedAccessException => CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status401Unauthorized,
                    "Unauthorized",
                    "You do not have permission to perform this operation."),
                KeyNotFoundException => CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status404NotFound,
                    "Not Found",
                    exception.Message),
                InvalidOperationException => CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status400BadRequest,
                    "Bad Request",
                    exception.Message),
                _ => CreateProblemDetails(
                    httpContext,
                    StatusCodes.Status500InternalServerError,
                    "Internal Server Error",
                    "An unexpected error occurred. Please try again later.")
            };

            httpContext.Response.StatusCode = problemDetails.Status!.Value;
            await httpContext.Response.WriteAsJsonAsync(problemDetails, cancellationToken);

            return true;
        }

        private static ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int status,
            string title,
            string detail)
        {
            return new ProblemDetails
            {
                Status = status,
                Title = title,
                Detail = detail,
                Instance = httpContext.Request.Path
            };
        }

        private static ProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ValidationException validationException)
        {
            var errors = validationException.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(
                    g => g.Key,
                    g => g.Select(e => e.ErrorMessage).ToArray());

            return new ProblemDetails
            {
                Status = StatusCodes.Status400BadRequest,
                Title = "Validation Error",
                Detail = "One or more validation errors occurred.",
                Instance = httpContext.Request.Path,
                Extensions = { ["errors"] = errors }
            };
        }
    }
}