using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.DTOs.Rental;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class RentalsController : ControllerBase
{
    private readonly IRentalService _rentalService;

    public RentalsController(IRentalService rentalService)
    {
        _rentalService = rentalService;
    }

    /// <summary>
    /// Giao xe - Check-out (Nhân viên)
    /// </summary>
    [HttpPost("checkout")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 400)]
    public async Task<IActionResult> Checkout([FromBody] CreateRentalRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // UserId will be fetched from booking if BookingId exists
        // Otherwise, use UserId from request (for walk-in customers)
        var userId = request.UserId ?? 0;

        var result = await _rentalService.CreateRentalAsync(userId, staffId, request);

        if (result == null)
        {
            return BadRequest(ApiResponse<RentalResponse>.ErrorResponse("Không thể tạo giao dịch thuê xe"));
        }

        return Ok(ApiResponse<RentalResponse>.SuccessResponse(result, "Giao xe thành công"));
    }

    /// <summary>
    /// Nhận xe trả - Check-in (Nhân viên)
    /// </summary>
    [HttpPost("{id}/checkin")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 400)]
    public async Task<IActionResult> Checkin(int id, [FromBody] CompleteRentalRequest request)
    {
        // Ensure rentalId in request matches route id
        if (request.RentalId != id)
        {
            return BadRequest(ApiResponse<RentalResponse>.ErrorResponse("ID giao dịch không khớp"));
        }

        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _rentalService.CompleteRentalAsync(staffId, request);
        
        if (result == null)
        {
            return BadRequest(ApiResponse<RentalResponse>.ErrorResponse("Không thể hoàn tất giao dịch thuê xe"));
        }

        return Ok(ApiResponse<RentalResponse>.SuccessResponse(result, "Nhận xe trả thành công"));
    }

    /// <summary>
    /// Lấy thông tin giao dịch thuê xe theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var rental = await _rentalService.GetRentalByIdAsync(id);
        
        if (rental == null)
        {
            return NotFound(ApiResponse<RentalResponse>.ErrorResponse("Không tìm thấy giao dịch thuê xe"));
        }

        return Ok(ApiResponse<RentalResponse>.SuccessResponse(rental));
    }

    /// <summary>
    /// Lấy lịch sử thuê xe của người dùng
    /// </summary>
    [HttpGet("my-rentals")]
    [Authorize(Roles = "Renter")]
    [ProducesResponseType(typeof(ApiResponse<List<RentalResponse>>), 200)]
    public async Task<IActionResult> GetMyRentals()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var rentals = await _rentalService.GetUserRentalsAsync(userId);
        return Ok(ApiResponse<List<RentalResponse>>.SuccessResponse(rentals));
    }

    /// <summary>
    /// Lấy danh sách giao dịch thuê xe đang hoạt động (Nhân viên/Admin)
    /// </summary>
    [HttpGet("active")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<RentalResponse>>), 200)]
    public async Task<IActionResult> GetActive([FromQuery] int? stationId = null)
    {
        var rentals = await _rentalService.GetActiveRentalsAsync(stationId);
        return Ok(ApiResponse<List<RentalResponse>>.SuccessResponse(rentals));
    }

    /// <summary>
    /// Lấy lịch sử check-in/check-out của một rental (ảnh, ghi chú)
    /// </summary>
    [HttpGet("{id}/inspections")]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), 200)]
    [ProducesResponseType(typeof(ApiResponse<List<object>>), 404)]
    public async Task<IActionResult> GetInspections(int id)
    {
        var rental = await _rentalService.GetRentalByIdAsync(id);
        if (rental == null)
        {
            return NotFound(ApiResponse<List<object>>.ErrorResponse("Không tìm thấy giao dịch thuê xe"));
        }

        var inspections = await _rentalService.GetRentalInspectionsAsync(id);
        return Ok(ApiResponse<List<object>>.SuccessResponse(inspections));
    }

    /// <summary>
    /// Lấy danh sách rentals tại điểm thuê (Staff)
    /// </summary>
    [HttpGet("station/{stationId}")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<RentalResponse>>), 200)]
    public async Task<IActionResult> GetByStation(int stationId, [FromQuery] string? status = null)
    {
        var rentals = await _rentalService.GetStationRentalsAsync(stationId, status);
        return Ok(ApiResponse<List<RentalResponse>>.SuccessResponse(rentals));
    }
}

