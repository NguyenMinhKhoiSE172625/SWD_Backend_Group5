using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.DTOs.Maintenance;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "StationStaff,Admin")]
[ValidateModel]
public class MaintenanceController : ControllerBase
{
    private readonly IMaintenanceService _maintenanceService;

    public MaintenanceController(IMaintenanceService maintenanceService)
    {
        _maintenanceService = maintenanceService;
    }

    /// <summary>
    /// Lên lịch bảo trì xe (Nhân viên/Admin)
    /// </summary>
    [HttpPost("schedule")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceScheduleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceScheduleResponse>), 400)]
    public async Task<IActionResult> CreateSchedule([FromBody] CreateMaintenanceScheduleRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _maintenanceService.CreateScheduleAsync(staffId, request);

        if (result == null)
        {
            return BadRequest(ApiResponse<MaintenanceScheduleResponse>.ErrorResponse("Không thể tạo lịch bảo trì"));
        }

        return Ok(ApiResponse<MaintenanceScheduleResponse>.SuccessResponse(result, "Tạo lịch bảo trì thành công"));
    }

    /// <summary>
    /// Cập nhật trạng thái lịch bảo trì (Nhân viên/Admin)
    /// </summary>
    [HttpPut("schedule")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceScheduleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceScheduleResponse>), 400)]
    public async Task<IActionResult> UpdateSchedule([FromBody] UpdateMaintenanceScheduleRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _maintenanceService.UpdateScheduleAsync(staffId, request);

        if (result == null)
        {
            return BadRequest(ApiResponse<MaintenanceScheduleResponse>.ErrorResponse("Không thể cập nhật lịch bảo trì"));
        }

        return Ok(ApiResponse<MaintenanceScheduleResponse>.SuccessResponse(result, "Cập nhật lịch bảo trì thành công"));
    }

    /// <summary>
    /// Lấy lịch bảo trì của xe (Nhân viên/Admin)
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/schedules")]
    [ProducesResponseType(typeof(ApiResponse<List<MaintenanceScheduleResponse>>), 200)]
    public async Task<IActionResult> GetVehicleSchedules(int vehicleId)
    {
        var schedules = await _maintenanceService.GetVehicleSchedulesAsync(vehicleId);
        return Ok(ApiResponse<List<MaintenanceScheduleResponse>>.SuccessResponse(schedules));
    }

    /// <summary>
    /// Lấy danh sách lịch bảo trì sắp tới (Nhân viên/Admin)
    /// </summary>
    [HttpGet("upcoming")]
    [ProducesResponseType(typeof(ApiResponse<List<MaintenanceScheduleResponse>>), 200)]
    public async Task<IActionResult> GetUpcomingSchedules([FromQuery] int? stationId = null)
    {
        var schedules = await _maintenanceService.GetUpcomingSchedulesAsync(stationId);
        return Ok(ApiResponse<List<MaintenanceScheduleResponse>>.SuccessResponse(schedules));
    }

    /// <summary>
    /// Tạo bản ghi bảo trì (hoàn tất bảo trì) (Nhân viên/Admin)
    /// </summary>
    [HttpPost("complete")]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceRecordResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<MaintenanceRecordResponse>), 400)]
    public async Task<IActionResult> CreateRecord([FromBody] CreateMaintenanceRecordRequest request)
    {
        var technicianId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _maintenanceService.CreateRecordAsync(technicianId, request);

        if (result == null)
        {
            return BadRequest(ApiResponse<MaintenanceRecordResponse>.ErrorResponse("Không thể tạo bản ghi bảo trì"));
        }

        return Ok(ApiResponse<MaintenanceRecordResponse>.SuccessResponse(result, "Hoàn tất bảo trì thành công"));
    }

    /// <summary>
    /// Lấy lịch sử bảo trì của xe (Nhân viên/Admin)
    /// </summary>
    [HttpGet("vehicle/{vehicleId}/records")]
    [ProducesResponseType(typeof(ApiResponse<List<MaintenanceRecordResponse>>), 200)]
    public async Task<IActionResult> GetVehicleRecords(int vehicleId)
    {
        var records = await _maintenanceService.GetVehicleRecordsAsync(vehicleId);
        return Ok(ApiResponse<List<MaintenanceRecordResponse>>.SuccessResponse(records));
    }
}

