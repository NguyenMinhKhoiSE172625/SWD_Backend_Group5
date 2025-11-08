using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/staff/dashboard")]
[Authorize(Roles = "StationStaff,Admin")]
[ValidateModel]
public class StaffDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IRentalService _rentalService;
    private readonly IBookingService _bookingService;
    private readonly IVehicleService _vehicleService;

    public StaffDashboardController(
        ApplicationDbContext context,
        IRentalService rentalService,
        IBookingService bookingService,
        IVehicleService vehicleService)
    {
        _context = context;
        _rentalService = rentalService;
        _bookingService = bookingService;
        _vehicleService = vehicleService;
    }

    /// <summary>
    /// Lấy dữ liệu dashboard cho nhân viên tại điểm thuê
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetDashboard([FromQuery] int? stationId = null)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        // If staff, use their station. If admin, use provided stationId or all stations
        var targetStationId = user?.StationId ?? stationId;

        if (targetStationId == null && user?.Role.ToString() != "Admin")
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Nhân viên chưa được gán vào điểm thuê"));
        }

        // Get vehicles at station
        var vehicles = targetStationId.HasValue
            ? await _vehicleService.GetStationVehiclesAsync(targetStationId.Value)
            : new List<Application.DTOs.Vehicle.VehicleResponse>();

        var availableVehicles = vehicles.Count(v => v.Status == "Available");
        var inUseVehicles = vehicles.Count(v => v.Status == "InUse");
        var bookedVehicles = vehicles.Count(v => v.Status == "Booked");
        var damagedVehicles = vehicles.Count(v => v.Status == "Damaged");

        // Get bookings at station
        var bookings = targetStationId.HasValue
            ? await _bookingService.GetStationBookingsAsync(targetStationId.Value)
            : new List<Application.DTOs.Booking.BookingResponse>();

        var pendingBookings = bookings.Count(b => b.Status == "Pending");
        var confirmedBookings = bookings.Count(b => b.Status == "Confirmed");
        var todayBookings = bookings.Count(b => 
            b.CreatedAt.Date == DateTime.UtcNow.Date);

        // Get active rentals at station
        var activeRentals = await _rentalService.GetActiveRentalsAsync(targetStationId);
        var todayRentals = activeRentals.Count(r => 
            r.PickupTime.Date == DateTime.UtcNow.Date);

        // Get unverified users at station
        var unverifiedUsers = await _context.Users
            .Where(u => u.Role == Domain.Enums.UserRole.Renter && !u.IsVerified)
            .CountAsync();

        var dashboard = new
        {
            StationId = targetStationId,
            Vehicles = new
            {
                Total = vehicles.Count,
                Available = availableVehicles,
                InUse = inUseVehicles,
                Booked = bookedVehicles,
                Damaged = damagedVehicles
            },
            Bookings = new
            {
                Total = bookings.Count,
                Pending = pendingBookings,
                Confirmed = confirmedBookings,
                Today = todayBookings
            },
            Rentals = new
            {
                Active = activeRentals.Count,
                Today = todayRentals
            },
            UnverifiedUsers = unverifiedUsers,
            RecentBookings = bookings
                .OrderByDescending(b => b.CreatedAt)
                .Take(5)
                .ToList(),
            RecentRentals = activeRentals
                .OrderByDescending(r => r.PickupTime)
                .Take(5)
                .ToList()
        };

        return Ok(ApiResponse<object>.SuccessResponse(dashboard));
    }

    /// <summary>
    /// Lấy danh sách users chưa verify tại điểm thuê
    /// </summary>
    [HttpGet("unverified-users")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetUnverifiedUsers()
    {
        var unverifiedUsers = await _context.Users
            .Where(u => u.Role == Domain.Enums.UserRole.Renter && !u.IsVerified)
            .Select(u => new
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                DriverLicenseNumber = u.DriverLicenseNumber,
                IdCardNumber = u.IdCardNumber,
                DriverLicenseImageUrl = u.DriverLicenseImageUrl,
                IdCardImageUrl = u.IdCardImageUrl,
                CreatedAt = u.CreatedAt
            })
            .OrderByDescending(u => u.CreatedAt)
            .ToListAsync();

        return Ok(ApiResponse<object>.SuccessResponse(unverifiedUsers));
    }
}

