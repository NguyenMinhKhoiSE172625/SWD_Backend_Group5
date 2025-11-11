using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Booking;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class BookingsController : ControllerBase
{
    private readonly IBookingService _bookingService;
    private readonly ApplicationDbContext _context;

    public BookingsController(IBookingService bookingService, ApplicationDbContext context)
    {
        _bookingService = bookingService;
        _context = context;
    }

    /// <summary>
    /// Tạo đặt xe mới (Người thuê)
    /// </summary>
    [HttpPost]
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
    /// Lấy thông tin đặt xe với đầy đủ thông tin cho checkout (Nhân viên)
    /// </summary>
    [HttpGet("{id}/checkout-info")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetCheckoutInfo(int id)
    {
        var booking = await _bookingService.GetBookingByIdAsync(id);
        
        if (booking == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy đặt xe"));
        }

        // Get full booking with user and vehicle details
        var fullBooking = await _bookingService.GetBookingForCheckoutAsync(id);
        
        if (fullBooking == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy đặt xe"));
        }

        return Ok(ApiResponse<object>.SuccessResponse(fullBooking));
    }

    /// <summary>
    /// Lấy danh sách đặt xe (Renter: bookings của mình, Staff: bookings tại station)
    /// </summary>
    [HttpGet("my-bookings")]
    [Authorize(Roles = "Renter,StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<BookingResponse>>), 200)]
    public async Task<IActionResult> GetMyBookings()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var userRole = User.FindFirst(ClaimTypes.Role)?.Value;

        // Nếu là Staff hoặc Admin, trả về bookings tại station của họ
        if (userRole == "StationStaff" || userRole == "Admin")
        {
            var user = await _context.Users.FindAsync(userId);
            if (user?.StationId.HasValue == true)
            {
                var bookings = await _bookingService.GetStationBookingsAsync(user.StationId.Value);
                return Ok(ApiResponse<List<BookingResponse>>.SuccessResponse(bookings));
            }
            // Nếu Admin không có station, trả về tất cả bookings
            var allBookings = await _context.Bookings
                .Include(b => b.Vehicle)
                .ThenInclude(v => v.Station)
                .OrderByDescending(b => b.CreatedAt)
                .ToListAsync();
            var mappedBookings = allBookings.Select(b => new BookingResponse
            {
                Id = b.Id,
                BookingCode = b.BookingCode,
                VehicleId = b.VehicleId,
                VehicleName = $"{b.Vehicle?.Brand} {b.Vehicle?.Model}",
                LicensePlate = b.Vehicle?.LicensePlate ?? "",
                StationId = b.StationId,
                StationName = b.Vehicle?.Station?.Name ?? "",
                ScheduledPickupTime = b.ScheduledPickupTime,
                ScheduledReturnTime = b.ScheduledReturnTime,
                Status = b.Status.ToString(),
                Notes = b.Notes,
                CreatedAt = b.CreatedAt
            }).ToList();
            return Ok(ApiResponse<List<BookingResponse>>.SuccessResponse(mappedBookings));
        }

        // Nếu là Renter, trả về bookings của họ
        var userBookings = await _bookingService.GetUserBookingsAsync(userId);
        return Ok(ApiResponse<List<BookingResponse>>.SuccessResponse(userBookings));
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
    [HttpPut("{id}/cancel")]
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
    [HttpPut("{id}/confirm")]
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

