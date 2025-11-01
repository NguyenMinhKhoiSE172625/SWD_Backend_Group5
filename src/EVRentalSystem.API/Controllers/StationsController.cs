using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.DTOs.Station;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Mvc;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ValidateModel]
public class StationsController : ControllerBase
{
    private readonly IStationService _stationService;

    public StationsController(IStationService stationService)
    {
        _stationService = stationService;
    }

    /// <summary>
    /// Lấy danh sách tất cả điểm thuê
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<List<StationResponse>>), 200)]
    public async Task<IActionResult> GetAll()
    {
        var stations = await _stationService.GetAllStationsAsync();
        return Ok(ApiResponse<List<StationResponse>>.SuccessResponse(stations));
    }

    /// <summary>
    /// Lấy thông tin điểm thuê theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<StationResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<StationResponse>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var station = await _stationService.GetStationByIdAsync(id);
        
        if (station == null)
        {
            return NotFound(ApiResponse<StationResponse>.ErrorResponse("Không tìm thấy điểm thuê"));
        }

        return Ok(ApiResponse<StationResponse>.SuccessResponse(station));
    }

    /// <summary>
    /// Tìm điểm thuê gần vị trí hiện tại
    /// </summary>
    [HttpGet("nearby")]
    [ProducesResponseType(typeof(ApiResponse<List<StationResponse>>), 200)]
    public async Task<IActionResult> GetNearby([FromQuery] decimal latitude, [FromQuery] decimal longitude, [FromQuery] double radiusKm = 10)
    {
        var stations = await _stationService.GetNearbyStationsAsync(latitude, longitude, radiusKm);
        return Ok(ApiResponse<List<StationResponse>>.SuccessResponse(stations));
    }
}

