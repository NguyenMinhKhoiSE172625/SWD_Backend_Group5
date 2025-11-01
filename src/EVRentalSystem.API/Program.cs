using System.Text;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using EVRentalSystem.Infrastructure.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// Add DbContext
builder.Services.AddDbContext<ApplicationDbContext>(options =>
    options.UseSqlite(builder.Configuration.GetConnectionString("DefaultConnection")));

// Add Services
builder.Services.AddScoped<IJwtService, JwtService>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IBookingService, BookingService>();
builder.Services.AddScoped<IRentalService, RentalService>();
builder.Services.AddScoped<IVehicleService, VehicleService>();
builder.Services.AddScoped<IStationService, StationService>();
builder.Services.AddScoped<IPaymentService, PaymentService>();

// Add Controllers
builder.Services.AddControllers();

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

// Add CORS
builder.Services.AddCors(options =>
{
    options.AddPolicy("AllowAll", policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

// Add Swagger
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(c =>
{
    c.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "EV Rental System API",
        Version = "v1",
        Description = "API cho hệ thống thuê xe điện tại điểm thuê",
        Contact = new OpenApiContact
        {
            Name = "EV Rental System",
            Email = "support@evrentalsystem.com"
        }
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
        context.Database.EnsureCreated();
        DbInitializer.Initialize(context);
    }
    catch (Exception ex)
    {
        var logger = services.GetRequiredService<ILogger<Program>>();
        logger.LogError(ex, "An error occurred while seeding the database.");
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
