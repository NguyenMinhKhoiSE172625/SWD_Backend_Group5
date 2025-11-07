using System.Text;
using Asp.Versioning;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using EVRentalSystem.Infrastructure.Services;
using EVRentalSystem.API.Middleware;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;
using Serilog;

// Configure Serilog
Log.Logger = new LoggerConfiguration()
    .MinimumLevel.Information()
    .MinimumLevel.Override("Microsoft", Serilog.Events.LogEventLevel.Warning)
    .MinimumLevel.Override("Microsoft.EntityFrameworkCore", Serilog.Events.LogEventLevel.Warning)
    .Enrich.FromLogContext()
    .WriteTo.Console()
    .WriteTo.File(
        path: "logs/evrentalsystem-.log",
        rollingInterval: RollingInterval.Day,
        retainedFileCountLimit: 30,
        outputTemplate: "{Timestamp:yyyy-MM-dd HH:mm:ss.fff zzz} [{Level:u3}] {Message:lj}{NewLine}{Exception}")
    .CreateLogger();

try
{
    Log.Information("Starting EV Rental System API");

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    options.UseSqlServer(builder.Configuration.GetConnectionString("DefaultConnection"));
    // Suppress pending model changes warning (indexes will be applied on next migration)
    options.ConfigureWarnings(warnings =>
        warnings.Ignore(Microsoft.EntityFrameworkCore.Diagnostics.RelationalEventId.PendingModelChangesWarning));
});

// Add Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();
builder.Services.AddScoped<IMaintenanceService, MaintenanceService>();
builder.Services.AddScoped<IAdminService, AdminService>();

// Add Controllers
builder.Services.AddControllers();

// Add API Versioning
builder.Services.AddApiVersioning(options =>
{
    options.DefaultApiVersion = new ApiVersion(1, 0);
    options.AssumeDefaultVersionWhenUnspecified = true;
    options.ReportApiVersions = true;
    options.ApiVersionReader = new UrlSegmentApiVersionReader();
}).AddApiExplorer(options =>
{
    options.GroupNameFormat = "'v'VVV";
    options.SubstituteApiVersionInUrl = true;
});

// Add Response Compression
builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

// Add JWT Authentication
builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = builder.Configuration["Jwt:Issuer"],
            ValidAudience = builder.Configuration["Jwt:Audience"],
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(builder.Configuration["Jwt:Key"]!))
        };
    });

builder.Services.AddAuthorization();

// Add CORS - Configure based on environment
builder.Services.AddCors(options =>
{
    if (builder.Environment.IsDevelopment())
    {
        // Development: Allow all for easier testing
        options.AddPolicy("AllowAll", policy =>
        {
            policy.AllowAnyOrigin()
                  .AllowAnyMethod()
                  .AllowAnyHeader();
        });
    }
    else
    {
        // Production: Restrict to specific origins
        var allowedOrigins = builder.Configuration.GetSection("Cors:AllowedOrigins").Get<string[]>()
            ?? new[] { "https://yourdomain.com" };

        options.AddPolicy("AllowAll", policy =>
        {
            policy.WithOrigins(allowedOrigins)
                  .AllowAnyMethod()
                  .AllowAnyHeader()
                  .AllowCredentials();
        });
    }
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EV Rental System API",
        Version = "v1.0"
    });

    // Add JWT Authentication to Swagger
    c.AddSecurityDefinition("Bearer", new OpenApiSecurityScheme
    {
        Description = "JWT Authorization header using the Bearer scheme. Example: \"Authorization: Bearer {token}\"",
        Name = "Authorization",
        In = ParameterLocation.Header,
        Type = SecuritySchemeType.ApiKey,
        Scheme = "Bearer"
    });

    c.AddSecurityRequirement(new OpenApiSecurityRequirement
    {
        {
            new OpenApiSecurityScheme
            {
                Reference = new OpenApiReference
                {
                    Type = ReferenceType.SecurityScheme,
                    Id = "Bearer"
                }
            },
            Array.Empty<string>()
        }
    });
});

var app = builder.Build();

// Configure the HTTP request pipeline
if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI(c =>
    {
        c.SwaggerEndpoint("/swagger/v1/swagger.json", "EV Rental System API V1");
        c.RoutePrefix = "swagger"; // Set Swagger UI at /swagger
    });
}

// Add Response Compression (must be early in pipeline)
app.UseResponseCompression();

// Add Serilog request logging
app.UseSerilogRequestLogging();

// Add Input Sanitization (must be before authentication)
app.UseInputSanitization();

// Add Global Exception Handler (must be early in pipeline)
app.UseGlobalExceptionHandler();

// Disable HTTPS redirection in development for easier testing
// app.UseHttpsRedirection();
app.UseCors("AllowAll");
app.UseAuthentication();
app.UseAuthorization();
app.MapControllers();

// Seed database
using (var scope = app.Services.CreateScope())
{
    var services = scope.ServiceProvider;
    try
    {
        var context = services.GetRequiredService<ApplicationDbContext>();
        var logger = services.GetRequiredService<ILogger<Program>>();
        
        // Check database connection and create if not exists
        try
        {
            // Migrate() will automatically create the database if it doesn't exist
            // But first, check if we can connect to the SQL Server instance
            logger.LogInformation("🔍 Đang kiểm tra kết nối SQL Server...");
            
            // Try to connect to master database first to verify SQL Server is accessible
            var connectionString = builder.Configuration.GetConnectionString("DefaultConnection");
            if (string.IsNullOrEmpty(connectionString))
            {
                throw new InvalidOperationException("Connection string không được cấu hình!");
            }

            // Extract database name from connection string
            var dbName = "EVRentalSystemDB";
            var builder_conn = new Microsoft.Data.SqlClient.SqlConnectionStringBuilder(connectionString);
            if (!string.IsNullOrEmpty(builder_conn.InitialCatalog))
            {
                dbName = builder_conn.InitialCatalog;
            }

            logger.LogInformation("📦 Database: {DatabaseName}", dbName);
            logger.LogInformation("🔄 Đang tạo database và áp dụng migrations...");
            logger.LogInformation("   (Database sẽ được tạo tự động nếu chưa tồn tại)");

            // Migrate() will:
            // 1. Create the database if it doesn't exist
            // 2. Create __EFMigrationsHistory table if it doesn't exist
            // 3. Apply all pending migrations
            context.Database.Migrate();
            
            logger.LogInformation("✅ Database '{DatabaseName}' đã sẵn sàng!", dbName);
            logger.LogInformation("✅ Tất cả migrations đã được áp dụng thành công!");
            
            logger.LogInformation("🌱 Đang khởi tạo dữ liệu mẫu...");
            DbInitializer.Initialize(context);
            logger.LogInformation("✅ Dữ liệu mẫu đã được khởi tạo thành công!");
        }
        catch (Microsoft.Data.SqlClient.SqlException sqlEx)
        {
            logger.LogError(sqlEx, "❌ Lỗi kết nối SQL Server!");
            logger.LogError("📋 Chi tiết lỗi: {ErrorMessage}", sqlEx.Message);
            logger.LogError("📋 Error Number: {ErrorNumber}, State: {State}", sqlEx.Number, sqlEx.State);
            logger.LogError("💡 Giải pháp:");
            
            // Check for specific error types
            if (sqlEx.Number == 18456) // Login failed
            {
                logger.LogError("   🔐 Lỗi xác thực - Windows Authentication:");
                logger.LogError("      1. Đảm bảo Windows user hiện tại có quyền truy cập SQL Server");
                logger.LogError("      2. Trong SSMS, vào Security > Logins và thêm Windows user");
                logger.LogError("      3. Hoặc đổi sang SQL Server Authentication trong connection string");
                logger.LogError("      4. Current Windows User: {User}", Environment.UserName);
            }
            else if (sqlEx.Number == 2 || sqlEx.Number == 53) // Server not found
            {
                logger.LogError("   🌐 Server không tìm thấy:");
                logger.LogError("      1. Kiểm tra SQL Server đang chạy: Services.msc > SQL Server (SQLEXPRESS)");
                logger.LogError("      2. Kiểm tra SQL Server Browser service đang chạy");
                logger.LogError("      3. Thử các connection string khác:");
                logger.LogError("         - Server=.\\SQLEXPRESS");
                logger.LogError("         - Server=(local)\\SQLEXPRESS");
                logger.LogError("         - Server=localhost\\SQLEXPRESS");
            }
            else if (sqlEx.Number == 2714) // Object already exists
            {
                logger.LogWarning("   ⚠️ Database đã có bảng từ schema cũ!");
                logger.LogWarning("   📋 Database có thể đã được tạo thủ công hoặc từ project khác.");
                logger.LogWarning("   💡 Giải pháp:");
                logger.LogWarning("      1. Xóa database và tạo lại (nếu không có dữ liệu quan trọng):");
                logger.LogWarning("         - Trong SSMS: Click phải vào database > Delete");
                logger.LogWarning("         - Chạy lại ứng dụng để tự động tạo database mới");
                logger.LogWarning("      2. Hoặc tạo database mới với tên khác trong appsettings.json");
                logger.LogWarning("      3. Hoặc mark migrations là đã applied (advanced):");
                logger.LogWarning("         - Chạy: dotnet ef database update --project src/EVRentalSystem.Infrastructure --startup-project src/EVRentalSystem.API");
            }
            else if (sqlEx.Number == 262 || sqlEx.Number == 2760) // Permission denied / Database creation failed
            {
                logger.LogError("   🔒 Không có quyền tạo database!");
                logger.LogError("   💡 Giải pháp:");
                logger.LogError("      1. Đảm bảo Windows user có quyền 'dbcreator' hoặc 'sysadmin':");
                logger.LogError("         - Trong SSMS: Security > Logins > [Your User] > Server Roles");
                logger.LogError("         - Chọn 'dbcreator' hoặc 'sysadmin'");
                logger.LogError("      2. Hoặc tạo database thủ công trong SSMS:");
                logger.LogError("         - Right-click Databases > New Database > Name: EVRentalSystemDB");
                logger.LogError("         - Sau đó chạy lại ứng dụng để áp dụng migrations");
                logger.LogError("      3. Current Windows User: {User}", Environment.UserName);
            }
            else
            {
                logger.LogError("   1. Đảm bảo SQL Server hoặc SQL Server Express đã được cài đặt");
                logger.LogError("   2. Kiểm tra SQL Server đang chạy: Services.msc > SQL Server (SQLEXPRESS)");
                logger.LogError("   3. Kiểm tra Windows Authentication được bật trong SQL Server");
            }
            
            logger.LogError("   Connection String: {ConnectionString}", 
                builder.Configuration.GetConnectionString("DefaultConnection"));
            logger.LogError("   4. Thử kết nối bằng SQL Server Management Studio (SSMS) với Windows Authentication");
            logger.LogError("   5. Hoặc đổi sang SQLite cho development: Data Source=EVRentalSystem.db");
            // Don't throw - let the app start but without database functionality
        }
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "❌ Lỗi khi khởi tạo database: {ErrorMessage}", ex.Message);
        // Don't throw - let the app start but without database functionality
    }
}

// Get the URL and open Swagger automatically
var lifetime = app.Services.GetRequiredService<IHostApplicationLifetime>();
lifetime.ApplicationStarted.Register(() =>
{
    var urls = app.Urls;
    var swaggerUrl = $"{urls.FirstOrDefault() ?? "http://localhost:5085"}/swagger";

    Console.WriteLine();
    Console.WriteLine("========================================");
    Console.WriteLine("🚀 EV Rental System API đã khởi động!");
    Console.WriteLine("========================================");
    Console.WriteLine($"📖 Swagger UI: {swaggerUrl}");
    Console.WriteLine($"🌐 API Base URL: {urls.FirstOrDefault() ?? "http://localhost:5085"}");
    Console.WriteLine("========================================");
    Console.WriteLine();

    // Auto-open Swagger in browser (only in Development)
    if (app.Environment.IsDevelopment())
    {
        try
        {
            var psi = new System.Diagnostics.ProcessStartInfo
            {
                FileName = swaggerUrl,
                UseShellExecute = true
            };
            System.Diagnostics.Process.Start(psi);
            Console.WriteLine("✅ Đã tự động mở Swagger UI trong trình duyệt!");
            Console.WriteLine();
        }
        catch (Exception ex)
        {
            Console.WriteLine($"⚠️ Không thể tự động mở trình duyệt: {ex.Message}");
            Console.WriteLine($"   Vui lòng mở thủ công: {swaggerUrl}");
            Console.WriteLine();
        }
    }
});

app.Run();

}
catch (Exception ex)
{
    Log.Fatal(ex, "Application terminated unexpectedly");
}
finally
{
    Log.CloseAndFlush();
}
