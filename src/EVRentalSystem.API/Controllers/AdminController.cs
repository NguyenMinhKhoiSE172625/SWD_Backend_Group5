using EVRentalSystem.Application.DTOs.Admin;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
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
    /// Seed bookings data (Admin only - Development)
    /// </summary>
    [HttpPost("seed/bookings")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> SeedBookings([FromServices] ApplicationDbContext context)
    {
        // Check if bookings already exist
        if (await context.Bookings.AnyAsync())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Bookings đã tồn tại. Xóa hết bookings trước khi seed."));
        }

        // Get existing users and vehicles
        var users = await context.Users.Where(u => u.Role == UserRole.Renter).ToListAsync();
        var vehicles = await context.Vehicles.ToListAsync();
        var stations = await context.Stations.ToListAsync();

        if (!users.Any() || !vehicles.Any() || !stations.Any())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần có Users, Vehicles và Stations trước khi seed Bookings."));
        }

        // Find specific users or use first available renters
        var userA = users.FirstOrDefault(u => u.Email == "nguyenvana@gmail.com") ?? users[0];
        var userB = users.Count > 1 ? (users.FirstOrDefault(u => u.Email == "tranthib@gmail.com") ?? users[1]) : users[0];

        // Get stations
        var station1 = stations.FirstOrDefault() ?? stations[0];
        var station2 = stations.Count > 1 ? stations[1] : stations[0];
        var station3 = stations.Count > 2 ? stations[2] : stations[0];

        // Seed Bookings
        var bookings = new[]
        {
            new Booking
            {
                BookingCode = "BK001",
                UserId = userA.Id,
                VehicleId = vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddDays(-2),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(1),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(2),
                Status = BookingStatus.Pending,
                Notes = "Cần xe gấp cho công việc",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Booking
            {
                BookingCode = "BK002",
                UserId = userA.Id,
                VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddDays(-5),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(2),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(1),
                Status = BookingStatus.Confirmed,
                Notes = "Đã xác nhận, sẵn sàng nhận xe",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Booking
            {
                BookingCode = "BK003",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddDays(-3),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(5),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(3),
                Status = BookingStatus.Confirmed,
                Notes = "Thuê xe đi du lịch 3 ngày",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Booking
            {
                BookingCode = "BK004",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 3 ? vehicles[3].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddDays(-10),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(-8),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(-7),
                Status = BookingStatus.Completed,
                Notes = "Hoàn thành tốt",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Booking
            {
                BookingCode = "BK005",
                UserId = userA.Id,
                VehicleId = vehicles.Count > 4 ? vehicles[4].Id : vehicles[0].Id,
                StationId = station3.Id,
                BookingDate = DateTime.UtcNow.AddDays(-4),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(-3),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(-2),
                Status = BookingStatus.Cancelled,
                Notes = "Khách hủy vì thay đổi kế hoạch",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            },
            new Booking
            {
                BookingCode = "BK006",
                UserId = userA.Id,
                VehicleId = vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddDays(-15),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(-14),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(-13),
                Status = BookingStatus.Completed,
                Notes = "Khách hàng thân thiết",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-13)
            },
            new Booking
            {
                BookingCode = "BK007",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddHours(-6),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(2),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(4),
                Status = BookingStatus.Pending,
                Notes = "Đặt trước cho tuần sau",
                CreatedAt = DateTime.UtcNow.AddHours(-6)
            }
        };

        context.Bookings.AddRange(bookings);
        await context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Seed bookings thành công!",
            count = bookings.Length
        }));
    }
}

