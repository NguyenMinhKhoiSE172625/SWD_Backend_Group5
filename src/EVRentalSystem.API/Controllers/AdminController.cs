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
