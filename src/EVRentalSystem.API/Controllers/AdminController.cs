using EVRentalSystem.Application.DTOs.Admin;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Lấy dữ liệu dashboard tổng quan (Admin)
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(ApiResponse<DashboardResponse>.SuccessResponse(dashboard));
    }

    /// <summary>
    /// Báo cáo doanh thu theo khoảng thời gian (Admin)
    /// </summary>
    [HttpGet("reports/revenue")]
    [ProducesResponseType(typeof(ApiResponse<List<RevenueReportResponse>>), 200)]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var report = await _adminService.GetRevenueReportAsync(startDate, endDate);
        return Ok(ApiResponse<List<RevenueReportResponse>>.SuccessResponse(report));
    }

    /// <summary>
    /// Báo cáo tình trạng và hiệu suất xe (Admin)
    /// </summary>
    [HttpGet("reports/vehicles")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleUtilizationResponse>>), 200)]
    public async Task<IActionResult> GetVehicleUtilization()
    {
        var report = await _adminService.GetVehicleUtilizationAsync();
        return Ok(ApiResponse<List<VehicleUtilizationResponse>>.SuccessResponse(report));
    }

    /// <summary>
    /// Báo cáo người dùng (Admin)
    /// </summary>
    [HttpGet("reports/users")]
    [ProducesResponseType(typeof(ApiResponse<UserReportResponse>), 200)]
    public async Task<IActionResult> GetUserReport()
    {
        var report = await _adminService.GetUserReportAsync();
        return Ok(ApiResponse<UserReportResponse>.SuccessResponse(report));
    }

    /// <summary>
    /// Phân tích đặt xe (Admin)
    /// </summary>
    [HttpGet("analytics/bookings")]
    [ProducesResponseType(typeof(ApiResponse<BookingAnalyticsResponse>), 200)]
    public async Task<IActionResult> GetBookingAnalytics()
    {
        var analytics = await _adminService.GetBookingAnalyticsAsync();
        return Ok(ApiResponse<BookingAnalyticsResponse>.SuccessResponse(analytics));
    }

    /// <summary>
    /// Danh sách xe được thuê nhiều nhất (Admin)
    /// </summary>
    [HttpGet("analytics/popular-vehicles")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleUtilizationResponse>>), 200)]
    public async Task<IActionResult> GetPopularVehicles([FromQuery] int topCount = 10)
    {
        var vehicles = await _adminService.GetPopularVehiclesAsync(topCount);
        return Ok(ApiResponse<List<VehicleUtilizationResponse>>.SuccessResponse(vehicles));
    }

    /// <summary>
    /// Tạo tài khoản staff mới (Admin)
    /// </summary>
    [HttpPost("staff")]
    [ProducesResponseType(typeof(ApiResponse<StaffResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> CreateStaff([FromBody] CreateStaffRequest request)
    {
        var staff = await _adminService.CreateStaffAsync(request);
        
        if (staff == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email đã tồn tại hoặc Station không hợp lệ"));
        }

        return Ok(ApiResponse<StaffResponse>.SuccessResponse(staff, "Tạo tài khoản staff thành công"));
    }

    /// <summary>
    /// Xóa tài khoản user (Admin)
    /// </summary>
    [HttpDelete("users/{userId}")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> DeleteUser([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("ID người dùng không hợp lệ"));
        }

        var result = await _adminService.DeleteUserAsync(userId);
        
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse(
                "Không thể xóa người dùng. User không tồn tại hoặc đang có booking/rental đang hoạt động"
            ));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xóa tài khoản thành công"));
    }

    /// <summary>
    /// Seed vehicles với booking tự động (Admin only - Development)
    /// </summary>
    [HttpPost("seed-vehicles")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> SeedVehicles([FromServices] ApplicationDbContext context)
    {
        var seededVehicles = new List<object>();
        var seededBookings = new List<object>();

        // Lấy station 1
        var station = await context.Stations.FindAsync(1);
        if (station == null)
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Không tìm thấy Station ID 1. Vui lòng tạo station trước."));
        }

        // Lấy danh sách users (renters) để tạo booking
        var renters = await context.Users
            .Where(u => u.Role == Domain.Enums.UserRole.Renter)
            .ToListAsync();
        
        if (!renters.Any())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Không có renter nào trong hệ thống. Vui lòng tạo user trước."));
        }

        var random = new Random();
        var vehicleData = new[]
        {
            new { Brand = "VinFast", Model = "Klara S", Year = 2023, Color = "Đỏ", BatteryCapacity = 100, PricePerHour = 30000m, PricePerDay = 200000m },
            new { Brand = "VinFast", Model = "Evo 200", Year = 2024, Color = "Xanh", BatteryCapacity = 100, PricePerHour = 35000m, PricePerDay = 250000m },
            new { Brand = "Honda", Model = "Vision", Year = 2023, Color = "Trắng", BatteryCapacity = 100, PricePerHour = 25000m, PricePerDay = 180000m },
            new { Brand = "Yamaha", Model = "Janus", Year = 2023, Color = "Đen", BatteryCapacity = 100, PricePerHour = 28000m, PricePerDay = 190000m },
            new { Brand = "VinFast", Model = "Theon S", Year = 2024, Color = "Xám", BatteryCapacity = 100, PricePerHour = 40000m, PricePerDay = 280000m },
            new { Brand = "VinFast", Model = "Feliz S", Year = 2023, Color = "Vàng", BatteryCapacity = 100, PricePerHour = 32000m, PricePerDay = 220000m },
            new { Brand = "Honda", Model = "SH Mode", Year = 2024, Color = "Xanh dương", BatteryCapacity = 100, PricePerHour = 38000m, PricePerDay = 260000m },
            new { Brand = "Yamaha", Model = "Sirius", Year = 2023, Color = "Đỏ đô", BatteryCapacity = 100, PricePerHour = 27000m, PricePerDay = 185000m },
            new { Brand = "VinFast", Model = "Ludo", Year = 2024, Color = "Hồng", BatteryCapacity = 100, PricePerHour = 33000m, PricePerDay = 230000m },
            new { Brand = "VinFast", Model = "Impes", Year = 2024, Color = "Bạc", BatteryCapacity = 100, PricePerHour = 45000m, PricePerDay = 300000m }
        };

        foreach (var vData in vehicleData)
        {
            var licensePlate = $"{random.Next(10, 99)}{(char)random.Next('A', 'Z' + 1)}{random.Next(1, 9)}-{random.Next(10000, 99999)}";

            // Kiểm tra biển số đã tồn tại chưa
            if (await context.Vehicles.AnyAsync(v => v.LicensePlate == licensePlate))
            {
                continue;
            }

            var vehicle = new Domain.Entities.Vehicle
            {
                LicensePlate = licensePlate,
                Model = vData.Model,
                Brand = vData.Brand,
                Year = vData.Year,
                Color = vData.Color,
                BatteryCapacity = vData.BatteryCapacity,
                PricePerHour = vData.PricePerHour,
                PricePerDay = vData.PricePerDay,
                Status = Domain.Enums.VehicleStatus.Booked, // Tất cả xe đều Booked
                ImageUrl = $"https://placehold.co/600x400/png?text={vData.Brand}+{vData.Model}",
                Description = $"Xe {vData.Brand} {vData.Model} {vData.Year} màu {vData.Color}",
                StationId = 1, // Chỉ thêm vào station 1
                CreatedAt = DateTime.UtcNow
            };

            context.Vehicles.Add(vehicle);
            await context.SaveChangesAsync(); // Save để có vehicle.Id

            // Tạo booking cho xe này
            var renter = renters[random.Next(renters.Count)];
            var scheduledPickupTime = DateTime.UtcNow.AddHours(random.Next(1, 48)); // 1-48 giờ tới
            var scheduledReturnTime = scheduledPickupTime.AddDays(random.Next(1, 7)); // 1-7 ngày sau

            var booking = new Domain.Entities.Booking
            {
                BookingCode = GenerateBookingCode(),
                UserId = renter.Id,
                VehicleId = vehicle.Id,
                StationId = 1, // Station 1
                BookingDate = DateTime.UtcNow,
                ScheduledPickupTime = scheduledPickupTime,
                ScheduledReturnTime = scheduledReturnTime,
                Status = Domain.Enums.BookingStatus.Confirmed, // Confirmed để logic đúng
                Notes = $"Booking tự động cho xe {vehicle.LicensePlate}",
                CreatedAt = DateTime.UtcNow
            };

            context.Bookings.Add(booking);
            await context.SaveChangesAsync();

            seededVehicles.Add(new
            {
                Id = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Model = $"{vehicle.Brand} {vehicle.Model}",
                Status = vehicle.Status.ToString(),
                StationName = station.Name
            });

            seededBookings.Add(new
            {
                Id = booking.Id,
                BookingCode = booking.BookingCode,
                VehicleLicensePlate = vehicle.LicensePlate,
                CustomerName = renter.FullName,
                ScheduledPickupTime = booking.ScheduledPickupTime,
                Status = booking.Status.ToString()
            });
        }

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            Message = $"Đã seed {seededVehicles.Count} xe và {seededBookings.Count} booking",
            Vehicles = seededVehicles,
            Bookings = seededBookings
        }));
    }

    private string GenerateBookingCode()
    {
        var randomBytes = new byte[2];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        var randomNumber = Math.Abs(BitConverter.ToInt16(randomBytes, 0)) % 9000 + 1000;
        return $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{randomNumber}";
    }

    /// <summary>
    /// Test email service - Gửi email test (Admin only - Development)
    /// </summary>
    [HttpPost("test-email")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> TestEmail(
        [FromServices] IEmailService emailService,
        [FromQuery] string toEmail,
        [FromQuery] string type = "reset")
    {
        if (string.IsNullOrEmpty(toEmail))
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Email là bắt buộc"));
        }

        bool result;
        string message;

        switch (type.ToLower())
        {
            case "reset":
                result = await emailService.SendPasswordResetEmailAsync(
                    toEmail, 
                    "TEST-TOKEN-123456", 
                    "Test User"
                );
                message = "Test email reset password";
                break;

            case "welcome":
                result = await emailService.SendWelcomeEmailAsync(toEmail, "Test User");
                message = "Test email chào mừng";
                break;

            case "booking":
                result = await emailService.SendBookingConfirmationEmailAsync(
                    toEmail, 
                    "Test User", 
                    "BK20241115001"
                );
                message = "Test email xác nhận đặt xe";
                break;

            default:
                return BadRequest(ApiResponse<object>.ErrorResponse(
                    "Type không hợp lệ. Chọn: reset, welcome, hoặc booking"
                ));
        }

        if (result)
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                EmailSent = true,
                ToEmail = toEmail,
                Type = type,
                Message = $"{message} đã được gửi thành công"
            }));
        }
        else
        {
            return Ok(ApiResponse<object>.SuccessResponse(new
            {
                EmailSent = false,
                ToEmail = toEmail,
                Type = type,
                Message = "Email service chưa được cấu hình. Kiểm tra console log."
            }));
        }
    }
}
