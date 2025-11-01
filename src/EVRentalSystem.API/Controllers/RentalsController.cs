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
    /// Tạo giao dịch thuê xe - Giao xe (Nhân viên)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateRentalRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        // For walk-in customers, userId should be provided in the request
        // For now, we'll use a simple approach - you might want to add userId to the request
        var userId = request.BookingId.HasValue 
            ? 0 // Will be fetched from booking
            : int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        
        var result = await _rentalService.CreateRentalAsync(userId, staffId, request);
        
        if (result == null)
        {
            return BadRequest(ApiResponse<RentalResponse>.ErrorResponse("Không thể tạo giao dịch thuê xe"));
        }

        return Ok(ApiResponse<RentalResponse>.SuccessResponse(result, "Giao xe thành công"));
    }

    /// <summary>
    /// Hoàn tất giao dịch thuê xe - Nhận xe trả (Nhân viên)
    /// </summary>
    [HttpPost("complete")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<RentalResponse>), 400)]
    public async Task<IActionResult> Complete([FromBody] CompleteRentalRequest request)
    {
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
    public async Task<IActionResult> GetActive()
    {
        var rentals = await _rentalService.GetActiveRentalsAsync();
        return Ok(ApiResponse<List<RentalResponse>>.SuccessResponse(rentals));
    }
}

