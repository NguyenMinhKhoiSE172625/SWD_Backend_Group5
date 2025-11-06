using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;

namespace EVRentalSystem.API.Middleware;

/// <summary>
/// Middleware to sanitize input and prevent XSS attacks
/// </summary>
public class InputSanitizationMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<InputSanitizationMiddleware> _logger;

    // Patterns to detect potential XSS attacks
    private static readonly Regex[] XssPatterns = new[]
    {
        new Regex(@"<script[^>]*>.*?</script>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"javascript:", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"on\w+\s*=", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<iframe[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<object[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled),
        new Regex(@"<embed[^>]*>", RegexOptions.IgnoreCase | RegexOptions.Compiled)
    };

    public InputSanitizationMiddleware(RequestDelegate next, ILogger<InputSanitizationMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        // Only sanitize POST, PUT, PATCH requests with JSON content
        if ((context.Request.Method == "POST" || 
             context.Request.Method == "PUT" || 
             context.Request.Method == "PATCH") &&
            context.Request.ContentType?.Contains("application/json") == true)
        {
            context.Request.EnableBuffering();

            using var reader = new StreamReader(
                context.Request.Body,
                encoding: Encoding.UTF8,
                detectEncodingFromByteOrderMarks: false,
                leaveOpen: true);

            var body = await reader.ReadToEndAsync();
            context.Request.Body.Position = 0;

            // Check for XSS patterns
            if (ContainsXss(body))
            {
                _logger.LogWarning("Potential XSS attack detected in request body from {IP}", 
                    context.Connection.RemoteIpAddress);

                context.Response.StatusCode = 400;
                context.Response.ContentType = "application/json";
                
                var errorResponse = new
                {
                    success = false,
                    message = "Invalid input detected",
                    errors = new[] { "Request contains potentially malicious content" }
                };

                await context.Response.WriteAsJsonAsync(errorResponse);
                return;
            }
        }

        // Sanitize query string parameters
        if (context.Request.Query.Any())
        {
            foreach (var param in context.Request.Query)
            {
                if (ContainsXss(param.Value.ToString()))
                {
                    _logger.LogWarning("Potential XSS attack detected in query parameter '{Param}' from {IP}", 
                        param.Key, context.Connection.RemoteIpAddress);

                    context.Response.StatusCode = 400;
                    context.Response.ContentType = "application/json";
                    
                    var errorResponse = new
                    {
                        success = false,
                        message = "Invalid input detected",
                        errors = new[] { $"Query parameter '{param.Key}' contains potentially malicious content" }
                    };

                    await context.Response.WriteAsJsonAsync(errorResponse);
                    return;
                }
            }
        }

        await _next(context);
    }

    private bool ContainsXss(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return false;

        return XssPatterns.Any(pattern => pattern.IsMatch(input));
    }
}

/// <summary>
/// Extension method to register the input sanitization middleware
/// </summary>
public static class InputSanitizationMiddlewareExtensions
{
    public static IApplicationBuilder UseInputSanitization(this IApplicationBuilder app)
    {
        return app.UseMiddleware<InputSanitizationMiddleware>();
    }
}

