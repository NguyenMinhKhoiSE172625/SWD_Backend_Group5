using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Booking;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;

    public BookingsController(IBookingService bookingService)
    {
        _bookingService = bookingService;
    }

    /// <summary>
    /// Tạo đặt xe mới (Người thuê)
    /// </summary>
    [HttpPost("create")]
    [Authorize(Roles = "Renter")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<BookingResponse>), 400)]
    public async Task<IActionResult> Create([FromBody] CreateBookingRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _bookingService.CreateBookingAsync(userId, request);
        
        if (result == null)
        {
            return BadRequest(ApiResponse<BookingResponse>.ErrorResponse("Không thể đặt xe. Xe có thể đã được đặt hoặc không khả dụng"));
        }

        return Ok(ApiResponse<BookingResponse>.SuccessResponse(result, "Đặt xe thành công"));
    }

    /// <summary>
    /// Lấy thông tin đặt xe theo ID
    /// </summary>
    [HttpGet("{id}")]
    [ProducesResponseType(typeof(ApiResponse<BookingResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<BookingResponse>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        
        if (booking == null)
        {
            return NotFound(ApiResponse<BookingResponse>.ErrorResponse("Không tìm thấy đặt xe"));
        }

        return Ok(ApiResponse<BookingResponse>.SuccessResponse(booking));
    }

    /// <summary>
    /// Lấy danh sách đặt xe của người dùng
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize(Roles = "Renter")]
    [ProducesResponseType(typeof(ApiResponse<List<BookingResponse>>), 200)]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var bookings = await _bookingService.GetUserBookingsAsync(userId);
        return Ok(ApiResponse<List<BookingResponse>>.SuccessResponse(bookings));
    }

    /// <summary>
    /// Lấy danh sách đặt xe tại điểm thuê (Nhân viên)
    /// </summary>
    [HttpGet("station/{stationId}")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<BookingResponse>>), 200)]
    public async Task<IActionResult> GetStationBookings(int stationId)
    {
        var bookings = await _bookingService.GetStationBookingsAsync(stationId);
        return Ok(ApiResponse<List<BookingResponse>>.SuccessResponse(bookings));
    }

    /// <summary>
    /// Hủy đặt xe (Người thuê)
    /// </summary>
    [HttpPost("{id}/cancel")]
    [Authorize(Roles = "Renter")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> Cancel(int id)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _bookingService.CancelBookingAsync(id, userId);
        
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể hủy đặt xe"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Hủy đặt xe thành công"));
    }

    /// <summary>
    /// Xác nhận đặt xe (Nhân viên)
    /// </summary>
    [HttpPost("{id}/confirm")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> Confirm(int id)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _bookingService.ConfirmBookingAsync(id, staffId);
        
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Không thể xác nhận đặt xe"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xác nhận đặt xe thành công"));
    }
}

