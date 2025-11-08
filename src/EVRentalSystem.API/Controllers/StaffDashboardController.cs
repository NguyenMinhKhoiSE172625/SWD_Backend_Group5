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
        var user = await _context.Users
            .Include(u => u.Station)
            .FirstOrDefaultAsync(u => u.Id == userId);

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

        // Get vehicles with booking/rental info for dashboard list
        var vehiclesWithInfo = await GetVehiclesWithBookingRentalInfo(targetStationId);

        var dashboard = new
        {
            StationId = targetStationId,
            StationName = user?.Station?.Name,
            Vehicles = new
            {
                Total = vehicles.Count,
                Available = availableVehicles,
                Booked = bookedVehicles,
                InUse = inUseVehicles,
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
            VehicleList = vehiclesWithInfo
        };

        return Ok(ApiResponse<object>.SuccessResponse(dashboard));
    }

    /// <summary>
    /// Lấy danh sách xe với thông tin booking/rental (cho dashboard)
    /// </summary>
    [HttpGet("vehicles")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetVehicles([FromQuery] int? stationId = null, [FromQuery] string? status = null)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users
            .Include(u => u.Station)
            .FirstOrDefaultAsync(u => u.Id == userId);

        var targetStationId = user?.StationId ?? stationId;

        if (targetStationId == null && user?.Role.ToString() != "Admin")
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Nhân viên chưa được gán vào điểm thuê"));
        }

        var vehicles = await GetVehiclesWithBookingRentalInfo(targetStationId, status);
        return Ok(ApiResponse<object>.SuccessResponse(vehicles));
    }

    private async Task<List<object>> GetVehiclesWithBookingRentalInfo(int? stationId, string? status = null)
    {
        var query = _context.Vehicles
            .Include(v => v.Station)
            .AsQueryable();

        if (stationId.HasValue)
        {
            query = query.Where(v => v.StationId == stationId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<Domain.Enums.VehicleStatus>(status, out var vehicleStatus))
        {
            query = query.Where(v => v.Status == vehicleStatus);
        }

        var vehicles = await query.ToListAsync();
        var vehicleIds = vehicles.Select(v => v.Id).ToList();

        // Get active bookings with user info
        var activeBookings = await _context.Bookings
            .Include(b => b.User)
            .Where(b => vehicleIds.Contains(b.VehicleId) 
                && (b.Status == Domain.Enums.BookingStatus.Confirmed || b.Status == Domain.Enums.BookingStatus.Pending))
            .ToListAsync();

        // Get active rentals with user info
        var activeRentals = await _context.Rentals
            .Include(r => r.User)
            .Where(r => vehicleIds.Contains(r.VehicleId) 
                && r.Status == Domain.Enums.RentalStatus.Active)
            .ToListAsync();

        return vehicles.Select(v =>
        {
            var activeBooking = activeBookings
                .Where(b => b.VehicleId == v.Id)
                .OrderBy(b => b.ScheduledPickupTime)
                .FirstOrDefault();

            var activeRental = activeRentals
                .Where(r => r.VehicleId == v.Id)
                .FirstOrDefault();

            return new
            {
                Id = v.Id,
                LicensePlate = v.LicensePlate,
                Model = v.Model,
                Brand = v.Brand,
                Year = v.Year,
                Color = v.Color,
                BatteryCapacity = v.BatteryCapacity,
                PricePerHour = v.PricePerHour,
                PricePerDay = v.PricePerDay,
                Status = v.Status.ToString(),
                ImageUrl = v.ImageUrl,
                Description = v.Description,
                StationId = v.StationId,
                StationName = v.Station?.Name,
                // Booking info (nếu có)
                Booking = activeBooking != null ? new
                {
                    Id = activeBooking.Id,
                    CustomerName = activeBooking.User.FullName,
                    CustomerId = activeBooking.UserId,
                    ScheduledPickupTime = activeBooking.ScheduledPickupTime,
                    Status = activeBooking.Status.ToString()
                } : null,
                // Rental info (nếu có)
                Rental = activeRental != null ? new
                {
                    Id = activeRental.Id,
                    CustomerName = activeRental.User.FullName,
                    CustomerId = activeRental.UserId,
                    PickupTime = activeRental.PickupTime,
                    Status = activeRental.Status.ToString()
                } : null
            };
        }).Cast<object>().ToList();
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

