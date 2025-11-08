using System.Text;
using Asp.Versioning;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using EVRentalSystem.Infrastructure.Services;
using EVRentalSystem.API.Middleware;
using EVRentalSystem.API.Filters;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Http;
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

    // Đọc PORT từ Railway (Railway tự động set biến này)
    var port = Environment.GetEnvironmentVariable("PORT");
    if (!string.IsNullOrEmpty(port) && int.TryParse(port, out int portNumber))
    {
        // Railway sẽ tự động expose port này
        Environment.SetEnvironmentVariable("ASPNETCORE_URLS", $"http://+:{port}");
        Log.Information("🚂 Railway PORT detected: {Port}", port);
    }
    else
    {
        // Development hoặc local - sử dụng default
        Log.Information("🔧 Running in local/development mode");
    }

var builder = WebApplication.CreateBuilder(args);

// Add Serilog
builder.Host.UseSerilog();

// Add DbContext - Hỗ trợ SQL Server, PostgreSQL (Railway), và SQLite
builder.Services.AddDbContext<ApplicationDbContext>(options =>
{
    // Ưu tiên đọc DATABASE_URL từ Railway (Railway tự động inject biến này)
    var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
    var connectionString = databaseUrl 
        ?? builder.Configuration.GetConnectionString("DefaultConnection");
    
    // Log để debug
    if (!string.IsNullOrEmpty(databaseUrl))
    {
        Log.Information("✅ DATABASE_URL từ Railway: {DatabaseUrl}", 
            databaseUrl.Length > 50 ? databaseUrl.Substring(0, 50) + "..." : databaseUrl);
    }
    else
    {
        Log.Warning("⚠️ DATABASE_URL không được set! App sẽ sử dụng connection string từ appsettings.json");
        Log.Warning("💡 Để kết nối PostgreSQL trên Railway:");
        Log.Warning("   1. Tạo PostgreSQL service trên Railway");
        Log.Warning("   2. Kết nối PostgreSQL service với app service (Settings > Variables > Add Reference)");
        Log.Warning("   3. Hoặc set DATABASE_URL manually trong Environment Variables");
    }
    
    if (string.IsNullOrEmpty(connectionString))
    {
        throw new InvalidOperationException("Connection string không được cấu hình! Set DATABASE_URL env variable trên Railway hoặc DefaultConnection trong appsettings.json");
    }
    
    // Helper method để convert PostgreSQL URL format sang connection string
    static string ConvertPostgresUrlToConnectionString(string url)
    {
        try
        {
            // Format: postgresql://user:password@host:port/database
            var uri = new Uri(url);
            var host = uri.Host;
            var port = uri.Port > 0 ? uri.Port : 5432;
            var database = uri.AbsolutePath.TrimStart('/');
            var userInfo = uri.UserInfo.Split(':');
            var username = userInfo.Length > 0 ? userInfo[0] : "postgres";
            var password = userInfo.Length > 1 ? userInfo[1] : "";
            
            return $"Host={host};Port={port};Database={database};Username={username};Password={password};SSL Mode=Require;";
        }
        catch
        {
            return url; // Return original nếu không parse được
        }
    }
    
    // Detect database type từ connection string
    if (connectionString.Contains("Host=") || 
        connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) ||
        connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
    {
        // PostgreSQL (Railway, Supabase, etc.)
        // Convert URL format nếu cần
        if (connectionString.StartsWith("postgresql://", StringComparison.OrdinalIgnoreCase) || 
            connectionString.StartsWith("postgres://", StringComparison.OrdinalIgnoreCase))
        {
            connectionString = ConvertPostgresUrlToConnectionString(connectionString);
        }
        
        // Cần import Npgsql.EntityFrameworkCore.PostgreSQL package
        // dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL
        try
        {
            options.UseNpgsql(connectionString);
            Log.Information("📊 Sử dụng PostgreSQL database");
        }
        catch (InvalidOperationException ex) when (ex.Message.Contains("Npgsql"))
        {
            Log.Warning("⚠️ Package Npgsql.EntityFrameworkCore.PostgreSQL chưa được cài đặt. Chạy: dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL");
            throw;
        }
    }
    else if (connectionString.Contains("Data Source=") && 
             (connectionString.EndsWith(".db", StringComparison.OrdinalIgnoreCase) || 
              connectionString.Contains(".db;")))
    {
        // SQLite (Local development)
        options.UseSqlite(connectionString);
        Log.Information("📊 Sử dụng SQLite database");
    }
    else
    {
        // SQL Server (Azure, Local, etc.) - Default
        options.UseSqlServer(connectionString);
        Log.Information("📊 Sử dụng SQL Server database");
    }
    
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
// Hỗ trợ đọc từ env variables: JWT__KEY, JWT__ISSUER, JWT__AUDIENCE (double underscore)
// Hoặc từ appsettings.json: Jwt:Key, Jwt:Issuer, Jwt:Audience
var jwtKey = builder.Configuration["Jwt:Key"] 
    ?? Environment.GetEnvironmentVariable("JWT__KEY")
    ?? throw new InvalidOperationException("JWT Key chưa được cấu hình! Set Jwt:Key trong appsettings.json hoặc JWT__KEY env variable.");
var jwtIssuer = builder.Configuration["Jwt:Issuer"] 
    ?? Environment.GetEnvironmentVariable("JWT__ISSUER")
    ?? "EVRentalSystem";
var jwtAudience = builder.Configuration["Jwt:Audience"] 
    ?? Environment.GetEnvironmentVariable("JWT__AUDIENCE")
    ?? "EVRentalSystemUsers";

builder.Services.AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtIssuer,
            ValidAudience = jwtAudience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtKey))
        };
    });

Log.Information("🔐 JWT configured - Issuer: {Issuer}, Audience: {Audience}", jwtIssuer, jwtAudience);

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
        // Production: Allow all origins (có thể restrict sau nếu cần)
        // Railway sẽ cung cấp domain dạng: https://your-app.railway.app
        // Bạn có thể set CORS_ORIGINS env variable trên Railway để restrict origins cụ thể
        var corsOriginsEnv = Environment.GetEnvironmentVariable("CORS_ORIGINS");
        if (!string.IsNullOrEmpty(corsOriginsEnv))
        {
            // Nếu có set CORS_ORIGINS env variable, sử dụng nó
            var allowedOrigins = corsOriginsEnv.Split(',', StringSplitOptions.RemoveEmptyEntries | StringSplitOptions.TrimEntries);
            options.AddPolicy("AllowAll", policy =>
            {
                policy.WithOrigins(allowedOrigins)
                      .AllowAnyMethod()
                      .AllowAnyHeader()
                      .AllowCredentials();
            });
            Log.Information("🔒 CORS restricted to: {Origins}", string.Join(", ", allowedOrigins));
        }
        else
        {
            // Mặc định cho phép tất cả origins (cho Railway deployment)
            options.AddPolicy("AllowAll", policy =>
            {
                policy.AllowAnyOrigin()
                      .AllowAnyMethod()
                      .AllowAnyHeader();
            });
            Log.Information("🌐 CORS: Allow all origins (set CORS_ORIGINS env variable trên Railway để restrict)");
        }
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

    // Ignore IFormFile in schema generation to avoid Swagger errors
    c.SchemaFilter<FormFileSchemaFilter>();
});

var app = builder.Build();

// Configure the HTTP request pipeline
// Bật Swagger trong Development và Production (có thể tắt bằng env variable ENABLE_SWAGGER=false)
var enableSwagger = Environment.GetEnvironmentVariable("ENABLE_SWAGGER") != "false";
if (app.Environment.IsDevelopment() || enableSwagger)
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

// Enable static files for uploaded files
app.UseStaticFiles();

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
            // Lấy connection string từ context đã được config (đã xử lý DATABASE_URL)
            var connectionString = context.Database.GetConnectionString();
            
            // Log để debug
            var databaseUrl = Environment.GetEnvironmentVariable("DATABASE_URL");
            var hasDatabaseUrl = !string.IsNullOrEmpty(databaseUrl);
            logger.LogInformation("🔍 DATABASE_URL env variable: {HasDatabaseUrl}", hasDatabaseUrl ? "✅ Đã được set" : "❌ Không có");
            
            if (hasDatabaseUrl)
            {
                logger.LogInformation("✅ Sử dụng DATABASE_URL từ Railway");
            }
            else
            {
                logger.LogWarning("⚠️ DATABASE_URL chưa được set, đang sử dụng connection string từ appsettings.json");
            }
            
            // Extract database name from connection string
            var dbName = "EVRentalSystemDB";
            if (!string.IsNullOrEmpty(connectionString))
            {
                if (connectionString.Contains("Database="))
                {
                    var dbMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Database=([^;]+)");
                    if (dbMatch.Success)
                    {
                        dbName = dbMatch.Groups[1].Value;
                    }
                }
                else if (connectionString.Contains("Initial Catalog="))
                {
                    var dbMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"Initial Catalog=([^;]+)");
                    if (dbMatch.Success)
                    {
                        dbName = dbMatch.Groups[1].Value;
                    }
                }
                else if (connectionString.Contains("postgresql://") || connectionString.Contains("postgres://"))
                {
                    // Extract from PostgreSQL URL format: postgresql://user:pass@host:port/database
                    var urlMatch = System.Text.RegularExpressions.Regex.Match(connectionString, @"(?:postgresql|postgres)://[^/]+/([^?;]+)");
                    if (urlMatch.Success)
                    {
                        dbName = urlMatch.Groups[1].Value;
                    }
                }
                
                // Log connection info (ẩn password)
                var maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
                    connectionString, 
                    @"(password|pwd)=[^;]+", 
                    "$1=***", 
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                maskedConnectionString = System.Text.RegularExpressions.Regex.Replace(
                    maskedConnectionString,
                    @"(?:postgresql|postgres)://[^:]+:[^@]+@",
                    "postgresql://***:***@",
                    System.Text.RegularExpressions.RegexOptions.IgnoreCase);
                logger.LogInformation("🔗 Connection String: {ConnectionString}", maskedConnectionString);
            }

            logger.LogInformation("🔍 Đang kiểm tra kết nối database...");
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
        catch (Exception dbEx) when (dbEx is Microsoft.Data.SqlClient.SqlException || 
                                      dbEx.GetType().FullName?.Contains("Npgsql") == true ||
                                      dbEx is Microsoft.Data.Sqlite.SqliteException)
        {
            logger.LogError(dbEx, "❌ Lỗi kết nối database!");
            logger.LogError("📋 Chi tiết lỗi: {ErrorMessage}", dbEx.Message);
            
            // Handle SQL Server errors
            if (dbEx is Microsoft.Data.SqlClient.SqlException sqlEx)
            {
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
            }
            // Handle PostgreSQL errors
            else if (dbEx.GetType().FullName?.Contains("Npgsql") == true)
            {
                logger.LogError("💡 Giải pháp cho PostgreSQL (Railway):");
                logger.LogError("   1. Kiểm tra connection string đúng chưa");
                logger.LogError("   2. Đảm bảo đã thêm 'SSL Mode=Require;' vào connection string");
                logger.LogError("   3. Kiểm tra database đã được tạo trên Railway chưa");
                logger.LogError("   4. Đảm bảo đã cài package: dotnet add src/EVRentalSystem.Infrastructure package Npgsql.EntityFrameworkCore.PostgreSQL");
                logger.LogError("   5. Xem hướng dẫn: manuals/RAILWAY_DEPLOYMENT_GUIDE.md");
            }
            // Handle SQLite errors
            else if (dbEx is Microsoft.Data.Sqlite.SqliteException)
            {
                logger.LogError("💡 Giải pháp cho SQLite:");
                logger.LogError("   1. Kiểm tra file database có tồn tại không");
                logger.LogError("   2. Kiểm tra quyền truy cập file");
                logger.LogError("   3. Đảm bảo đường dẫn đúng: Data Source=EVRentalSystem.db");
            }
            
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
