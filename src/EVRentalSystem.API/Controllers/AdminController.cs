using EVRentalSystem.Application.DTOs.Admin;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

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
}

