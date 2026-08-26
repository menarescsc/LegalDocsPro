using System.Net;
using FluentAssertions;
using FluentValidation;
using FluentValidation.Results;
using LegalDocsPro.Api.Middlewares;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;

namespace LegalDocsPro.Api.Tests.Middlewares;

public class GlobalExceptionHandlerTests
{
    private readonly GlobalExceptionHandler _handler;
    private readonly Mock<ILogger<GlobalExceptionHandler>> _loggerMock;

    public GlobalExceptionHandlerTests()
    {
        _loggerMock = new Mock<ILogger<GlobalExceptionHandler>>();
        _handler = new GlobalExceptionHandler(_loggerMock.Object);
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_Returns400WithFieldErrors()
    {
        var failures = new List<ValidationFailure>
        {
            new("Title", "Title is required."),
            new("Email", "Email is not valid."),
            new("Email", "Email must contain @.")
        };
        var exception = new ValidationException(failures);
        var httpContext = CreateHttpContext();

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(400);

        var body = await ReadProblemDetails(httpContext);
        body.Status.Should().Be(400);
        body.Title.Should().Be("Validation Error");
        body.Detail.Should().Be("One or more validation errors occurred.");
        body.Extensions.Should().ContainKey("errors");
    }

    [Fact]
    public async Task TryHandleAsync_UnauthorizedAccessException_Returns403()
    {
        var exception = new UnauthorizedAccessException("Access denied to resource.");
        var httpContext = CreateHttpContext();

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(403);

        var body = await ReadProblemDetails(httpContext);
        body.Status.Should().Be(403);
        body.Title.Should().Be("Forbidden");
        body.Detail.Should().Be("You do not have permission to perform this operation.");
    }

    [Fact]
    public async Task TryHandleAsync_KeyNotFoundException_Returns404()
    {
        var exception = new KeyNotFoundException("Contract with ID 42 not found.");
        var httpContext = CreateHttpContext();

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(404);

        var body = await ReadProblemDetails(httpContext);
        body.Status.Should().Be(404);
        body.Title.Should().Be("Not Found");
        body.Detail.Should().Be("Contract with ID 42 not found.");
    }

    [Fact]
    public async Task TryHandleAsync_InvalidOperationException_Returns400()
    {
        var exception = new InvalidOperationException("Only draft contracts can be sent to review.");
        var httpContext = CreateHttpContext();

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(400);

        var body = await ReadProblemDetails(httpContext);
        body.Status.Should().Be(400);
        body.Title.Should().Be("Business Rule Violation");
        body.Detail.Should().Be("Only draft contracts can be sent to review.");
    }

    [Fact]
    public async Task TryHandleAsync_GenericException_Returns500WithSanitizedMessage()
    {
        var exception = new Exception("Internal database connection string with password=secret123");
        var httpContext = CreateHttpContext();

        var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        result.Should().BeTrue();
        httpContext.Response.StatusCode.Should().Be(500);

        var body = await ReadProblemDetails(httpContext);
        body.Status.Should().Be(500);
        body.Title.Should().Be("Internal Server Error");
        body.Detail.Should().Be("An unexpected error occurred. Please try again later.");
        body.Detail.Should().NotContain("password");
        body.Detail.Should().NotContain("secret123");
    }

    [Fact]
    public async Task TryHandleAsync_Always_ReturnsTrueToIndicateHandled()
    {
        var exceptions = new Exception[]
        {
            new ValidationException("test"),
            new UnauthorizedAccessException(),
            new KeyNotFoundException(),
            new InvalidOperationException(),
            new Exception("generic")
        };

        foreach (var exception in exceptions)
        {
            var httpContext = CreateHttpContext();
            var result = await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);
            result.Should().BeTrue($"because {exception.GetType().Name} should be handled");
        }
    }

    [Fact]
    public async Task TryHandleAsync_ValidationException_GroupsErrorsByPropertyName()
    {
        var failures = new List<ValidationFailure>
        {
            new("Title", "Title is required."),
            new("Title", "Title must be between 1 and 200 characters."),
            new("Description", "Description is required.")
        };
        var exception = new ValidationException(failures);
        var httpContext = CreateHttpContext();

        await _handler.TryHandleAsync(httpContext, exception, CancellationToken.None);

        // Read raw JSON to verify the errors extension structure
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        using var doc = await System.Text.Json.JsonDocument.ParseAsync(httpContext.Response.Body);
        var root = doc.RootElement;

        root.GetProperty("status").GetInt32().Should().Be(400);
        root.GetProperty("title").GetString().Should().Be("Validation Error");

        var errors = root.GetProperty("errors");
        errors.GetProperty("Title").GetArrayLength().Should().Be(2);
        errors.GetProperty("Description").GetArrayLength().Should().Be(1);
    }

    private static DefaultHttpContext CreateHttpContext()
    {
        var httpContext = new DefaultHttpContext();
        httpContext.Response.Body = new MemoryStream();
        httpContext.Request.Path = "/api/test";
        return httpContext;
    }

    private static async Task<ProblemDetails> ReadProblemDetails(HttpContext httpContext)
    {
        httpContext.Response.Body.Seek(0, SeekOrigin.Begin);
        var body = await System.Text.Json.JsonSerializer.DeserializeAsync<ProblemDetails>(
            httpContext.Response.Body,
            new System.Text.Json.JsonSerializerOptions
            {
                PropertyNameCaseInsensitive = true
            });
        return body!;
    }
}
