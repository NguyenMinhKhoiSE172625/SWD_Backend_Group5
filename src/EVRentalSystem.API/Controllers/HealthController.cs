using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.API.Controllers;

/// <summary>
/// Health check endpoint for monitoring system status
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class HealthController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly ILogger<HealthController> _logger;

    public HealthController(ApplicationDbContext context, ILogger<HealthController> logger)
    {
        _context = context;
        _logger = logger;
    }

    /// <summary>
    /// Basic health check - returns 200 OK if service is running
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(HealthCheckResponse), 200)]
    [ProducesResponseType(typeof(HealthCheckResponse), 503)]
    public async Task<IActionResult> Get()
    {
        var response = new HealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        try
        {
            // Check database connectivity
            var canConnect = await _context.Database.CanConnectAsync();
            response.Database = canConnect ? "Connected" : "Disconnected";

            if (!canConnect)
            {
                response.Status = "Unhealthy";
                response.Errors.Add("Database connection failed");
                _logger.LogError("Health check failed: Database connection failed");
                return StatusCode(503, response);
            }

            // Check database response time
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            await _context.Users.Take(1).ToListAsync();
            stopwatch.Stop();
            response.DatabaseResponseTime = $"{stopwatch.ElapsedMilliseconds}ms";

            // Get system info
            response.Uptime = GetUptime();
            response.MemoryUsage = GetMemoryUsage();

            _logger.LogInformation("Health check passed");
            return Ok(response);
        }
        catch (Exception ex)
        {
            response.Status = "Unhealthy";
            response.Database = "Error";
            response.Errors.Add($"Health check failed: {ex.Message}");
            _logger.LogError(ex, "Health check failed with exception");
            return StatusCode(503, response);
        }
    }

    /// <summary>
    /// Detailed health check with component status
    /// </summary>
    [HttpGet("detailed")]
    [ProducesResponseType(typeof(DetailedHealthCheckResponse), 200)]
    public async Task<IActionResult> GetDetailed()
    {
        var response = new DetailedHealthCheckResponse
        {
            Status = "Healthy",
            Timestamp = DateTime.UtcNow,
            Version = "1.0.0",
            Environment = Environment.GetEnvironmentVariable("ASPNETCORE_ENVIRONMENT") ?? "Production"
        };

        // Check Database
        response.Components.Add("Database", await CheckDatabaseHealth());

        // Check API
        response.Components.Add("API", new ComponentHealth
        {
            Status = "Healthy",
            ResponseTime = "< 1ms"
        });

        // Overall status
        var hasUnhealthyComponent = response.Components.Values.Any(c => c.Status == "Unhealthy");
        response.Status = hasUnhealthyComponent ? "Degraded" : "Healthy";

        return Ok(response);
    }

    private async Task<ComponentHealth> CheckDatabaseHealth()
    {
        var component = new ComponentHealth();
        try
        {
            var stopwatch = System.Diagnostics.Stopwatch.StartNew();
            var canConnect = await _context.Database.CanConnectAsync();
            stopwatch.Stop();

            component.Status = canConnect ? "Healthy" : "Unhealthy";
            component.ResponseTime = $"{stopwatch.ElapsedMilliseconds}ms";

            if (canConnect)
            {
                var userCount = await _context.Users.CountAsync();
                var vehicleCount = await _context.Vehicles.CountAsync();
                var bookingCount = await _context.Bookings.CountAsync();

                component.Details = new Dictionary<string, object>
                {
                    { "Users", userCount },
                    { "Vehicles", vehicleCount },
                    { "Bookings", bookingCount }
                };
            }
        }
        catch (Exception ex)
        {
            component.Status = "Unhealthy";
            component.Error = ex.Message;
        }

        return component;
    }

    private string GetUptime()
    {
        var uptime = DateTime.UtcNow - System.Diagnostics.Process.GetCurrentProcess().StartTime.ToUniversalTime();
        return $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
    }

    private string GetMemoryUsage()
    {
        var process = System.Diagnostics.Process.GetCurrentProcess();
        var memoryMB = process.WorkingSet64 / 1024 / 1024;
        return $"{memoryMB} MB";
    }
}

public class HealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public string Database { get; set; } = "Unknown";
    public string DatabaseResponseTime { get; set; } = "N/A";
    public string Uptime { get; set; } = "N/A";
    public string MemoryUsage { get; set; } = "N/A";
    public List<string> Errors { get; set; } = new();
}

public class DetailedHealthCheckResponse
{
    public string Status { get; set; } = "Healthy";
    public DateTime Timestamp { get; set; }
    public string Version { get; set; } = string.Empty;
    public string Environment { get; set; } = string.Empty;
    public Dictionary<string, ComponentHealth> Components { get; set; } = new();
}

public class ComponentHealth
{
    public string Status { get; set; } = "Unknown";
    public string ResponseTime { get; set; } = "N/A";
    public string? Error { get; set; }
    public Dictionary<string, object>? Details { get; set; }
}

