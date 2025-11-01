using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.DTOs.Vehicle;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ValidateModel]
public class VehiclesController : ControllerBase
{
    private readonly IVehicleService _vehicleService;

    public VehiclesController(IVehicleService vehicleService)
    {
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// Lấy danh sách xe có sẵn
    /// </summary>
    [HttpGet("available")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponse>>), 200)]
    public async Task<IActionResult> GetAvailable([FromQuery] int? stationId = null)
    {
        var vehicles = await _vehicleService.GetAvailableVehiclesAsync(stationId);
        return Ok(ApiResponse<List<VehicleResponse>>.SuccessResponse(vehicles));
    }

    /// <summary>
    /// Lấy thông tin xe theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<VehicleResponse>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var vehicle = await _vehicleService.GetVehicleByIdAsync(id);
        
        if (vehicle == null)
        {
            return NotFound(ApiResponse<VehicleResponse>.ErrorResponse("Không tìm thấy xe"));
        }

        return Ok(ApiResponse<VehicleResponse>.SuccessResponse(vehicle));
    }

    /// <summary>
    /// Lấy danh sách xe tại điểm thuê (Chỉ nhân viên)
    /// </summary>
    [HttpGet("station/{stationId}")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleResponse>>), 200)]
    public async Task<IActionResult> GetByStation(int stationId)
    {
        var vehicles = await _vehicleService.GetStationVehiclesAsync(stationId);
        return Ok(ApiResponse<List<VehicleResponse>>.SuccessResponse(vehicles));
    }

    /// <summary>
    /// Cập nhật trạng thái xe (Chỉ nhân viên)
    /// </summary>
    [HttpPut("{id}/status")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> UpdateStatus(int id, [FromBody] UpdateVehicleStatusRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _vehicleService.UpdateVehicleStatusAsync(id, request.Status, staffId);
        
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể cập nhật trạng thái xe"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Cập nhật trạng thái thành công"));
    }

    /// <summary>
    /// Cập nhật mức pin xe (Chỉ nhân viên)
    /// </summary>
    [HttpPut("{id}/battery")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> UpdateBattery(int id, [FromBody] UpdateVehicleBatteryRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _vehicleService.UpdateVehicleBatteryAsync(id, request.BatteryLevel, staffId);
        
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể cập nhật mức pin"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Cập nhật mức pin thành công"));
    }
}

public class UpdateVehicleStatusRequest
{
    public string Status { get; set; } = string.Empty;
}

public class UpdateVehicleBatteryRequest
{
    public int BatteryLevel { get; set; }
}

