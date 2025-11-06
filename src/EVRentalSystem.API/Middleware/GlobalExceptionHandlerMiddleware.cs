using System.Net;
using System.Text.Json;
using EVRentalSystem.Application.DTOs.Common;

namespace EVRentalSystem.API.Middleware;

/// <summary>
/// Global exception handler middleware to catch all unhandled exceptions
/// and return standardized error responses
/// </summary>
public class GlobalExceptionHandlerMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<GlobalExceptionHandlerMiddleware> _logger;
    private readonly IHostEnvironment _environment;

    public GlobalExceptionHandlerMiddleware(
        RequestDelegate next,
        ILogger<GlobalExceptionHandlerMiddleware> logger,
        IHostEnvironment environment)
    {
        _next = next;
        _logger = logger;
        _environment = environment;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "An unhandled exception occurred: {Message}", ex.Message);
            await HandleExceptionAsync(context, ex);
        }
    }

    private async Task HandleExceptionAsync(HttpContext context, Exception exception)
    {
        context.Response.ContentType = "application/json";
        
        var response = exception switch
        {
            ArgumentNullException => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "Invalid request",
                "One or more required fields are missing",
                exception),
            
            ArgumentException => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "Invalid request",
                exception.Message,
                exception),
            
            UnauthorizedAccessException => CreateErrorResponse(
                HttpStatusCode.Unauthorized,
                "Unauthorized",
                "You are not authorized to access this resource",
                exception),
            
            KeyNotFoundException => CreateErrorResponse(
                HttpStatusCode.NotFound,
                "Resource not found",
                exception.Message,
                exception),
            
            InvalidOperationException => CreateErrorResponse(
                HttpStatusCode.BadRequest,
                "Invalid operation",
                exception.Message,
                exception),
            
            _ => CreateErrorResponse(
                HttpStatusCode.InternalServerError,
                "Internal server error",
                "An unexpected error occurred. Please try again later.",
                exception)
        };

        context.Response.StatusCode = (int)response.StatusCode;

        var jsonOptions = new JsonSerializerOptions
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            WriteIndented = true
        };

        var json = JsonSerializer.Serialize(response.ApiResponse, jsonOptions);
        await context.Response.WriteAsync(json);
    }

    private (HttpStatusCode StatusCode, ApiResponse<object> ApiResponse) CreateErrorResponse(
        HttpStatusCode statusCode,
        string message,
        string detail,
        Exception exception)
    {
        var errors = new List<string> { detail };

        // In development, include stack trace
        if (_environment.IsDevelopment() && exception != null)
        {
            errors.Add($"Exception Type: {exception.GetType().Name}");
            errors.Add($"Stack Trace: {exception.StackTrace}");
        }

        var apiResponse = ApiResponse<object>.ErrorResponse(message, errors);

        return (statusCode, apiResponse);
    }
}

/// <summary>
/// Extension method to register the global exception handler middleware
/// </summary>
public static class GlobalExceptionHandlerMiddlewareExtensions
{
    public static IApplicationBuilder UseGlobalExceptionHandler(this IApplicationBuilder app)
    {
        return app.UseMiddleware<GlobalExceptionHandlerMiddleware>();
    }
}

