using EVRentalSystem.Application.DTOs.Booking;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class BookingService : IBookingService
{
    private readonly ApplicationDbContext _context;

    public BookingService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<BookingResponse?> CreateBookingAsync(int userId, CreateBookingRequest request)
    {
        // Validate time inputs
        if (request.ScheduledPickupTime < DateTime.UtcNow)
        {
            throw new ArgumentException("Thời gian nhận xe phải là thời điểm trong tương lai");
        }

        if (request.ScheduledReturnTime.HasValue && request.ScheduledReturnTime <= request.ScheduledPickupTime)
        {
            throw new ArgumentException("Thời gian trả xe phải sau thời gian nhận xe");
        }

        // Validate reasonable rental duration (max 30 days)
        if (request.ScheduledReturnTime.HasValue)
        {
            var duration = request.ScheduledReturnTime.Value - request.ScheduledPickupTime;
            if (duration.TotalDays > 30)
            {
                throw new ArgumentException("Thời gian thuê tối đa là 30 ngày");
            }
        }

        // Use transaction to prevent race condition
        using var transaction = await _context.Database.BeginTransactionAsync();
        try
        {
            // Lock the vehicle row for update to prevent concurrent bookings
            var vehicle = await _context.Vehicles
                .Include(v => v.Station)
                .FirstOrDefaultAsync(v => v.Id == request.VehicleId);

            if (vehicle == null || vehicle.Status != VehicleStatus.Available)
            {
                return null;
            }

            // Check for overlapping bookings
            var hasOverlap = await _context.Bookings
                .Where(b => b.VehicleId == request.VehicleId
                    && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed)
                    && b.ScheduledPickupTime < (request.ScheduledReturnTime ?? request.ScheduledPickupTime.AddDays(1))
                    && (b.ScheduledReturnTime ?? b.ScheduledPickupTime.AddDays(1)) > request.ScheduledPickupTime)
                .AnyAsync();

            if (hasOverlap)
            {
                throw new InvalidOperationException("Xe đã được đặt trong khoảng thời gian này. Vui lòng chọn thời gian khác.");
            }

            var booking = new Booking
            {
                BookingCode = GenerateBookingCode(),
                UserId = userId,
                VehicleId = request.VehicleId,
                StationId = request.StationId,
                BookingDate = DateTime.UtcNow,
                ScheduledPickupTime = request.ScheduledPickupTime,
                ScheduledReturnTime = request.ScheduledReturnTime,
                Status = BookingStatus.Pending,
                Notes = request.Notes,
                CreatedAt = DateTime.UtcNow
            };

            vehicle.Status = VehicleStatus.Booked;
            vehicle.UpdatedAt = DateTime.UtcNow;

            _context.Bookings.Add(booking);
            await _context.SaveChangesAsync();
            await transaction.CommitAsync();

            return MapToResponse(booking, vehicle);
        }
        catch
        {
            await transaction.RollbackAsync();
            throw;
        }
    }

    public async Task<BookingResponse?> GetBookingByIdAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Vehicle)
            .ThenInclude(v => v.Station)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        return booking == null ? null : MapToResponse(booking, booking.Vehicle);
    }

    public async Task<List<BookingResponse>> GetUserBookingsAsync(int userId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Vehicle)
            .ThenInclude(v => v.Station)
            .Where(b => b.UserId == userId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => MapToResponse(b, b.Vehicle)).ToList();
    }

    public async Task<List<BookingResponse>> GetStationBookingsAsync(int stationId)
    {
        var bookings = await _context.Bookings
            .Include(b => b.Vehicle)
            .ThenInclude(v => v.Station)
            .Where(b => b.StationId == stationId)
            .OrderByDescending(b => b.CreatedAt)
            .ToListAsync();

        return bookings.Select(b => MapToResponse(b, b.Vehicle)).ToList();
    }

    public async Task<bool> CancelBookingAsync(int bookingId, int userId)
    {
        var booking = await _context.Bookings
            .Include(b => b.Vehicle)
            .FirstOrDefaultAsync(b => b.Id == bookingId && b.UserId == userId);

        if (booking == null || booking.Status != BookingStatus.Pending)
        {
            return false;
        }

        booking.Status = BookingStatus.Cancelled;
        booking.UpdatedAt = DateTime.UtcNow;

        // Only set vehicle to Available if there are no other active bookings
        var hasOtherBookings = await _context.Bookings
            .AnyAsync(b => b.VehicleId == booking.VehicleId
                && b.Id != bookingId
                && (b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed));

        if (!hasOtherBookings)
        {
            booking.Vehicle.Status = VehicleStatus.Available;
            booking.Vehicle.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();
        return true;
    }

    public async Task<bool> ConfirmBookingAsync(int bookingId, int staffId)
    {
        var booking = await _context.Bookings.FindAsync(bookingId);
        if (booking == null || booking.Status != BookingStatus.Pending)
        {
            return false;
        }

        booking.Status = BookingStatus.Confirmed;
        booking.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();
        return true;
    }

    private string GenerateBookingCode()
    {
        // Use RandomNumberGenerator for thread-safety and better randomness
        var randomBytes = new byte[2];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        var randomNumber = Math.Abs(BitConverter.ToInt16(randomBytes, 0)) % 9000 + 1000;
        return $"BK{DateTime.UtcNow:yyyyMMddHHmmss}{randomNumber}";
    }

    public async Task<object?> GetBookingForCheckoutAsync(int bookingId)
    {
        var booking = await _context.Bookings
            .Include(b => b.User)
            .Include(b => b.Vehicle)
                .ThenInclude(v => v.Station)
            .FirstOrDefaultAsync(b => b.Id == bookingId);

        if (booking == null)
        {
            return null;
        }

        return new
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            Vehicle = new
            {
                Id = booking.Vehicle.Id,
                LicensePlate = booking.Vehicle.LicensePlate,
                Model = booking.Vehicle.Model,
                Brand = booking.Vehicle.Brand,
                Year = booking.Vehicle.Year,
                Color = booking.Vehicle.Color,
                BatteryCapacity = booking.Vehicle.BatteryCapacity,
                ImageUrl = booking.Vehicle.ImageUrl,
                Description = booking.Vehicle.Description,
                VehicleName = $"{booking.Vehicle.Brand} {booking.Vehicle.Model}"
            },
            Station = new
            {
                Id = booking.StationId,
                Name = booking.Vehicle.Station?.Name ?? ""
            },
            Customer = new
            {
                Id = booking.User.Id,
                FullName = booking.User.FullName,
                Email = booking.User.Email,
                PhoneNumber = booking.User.PhoneNumber
            },
            ScheduledPickupTime = booking.ScheduledPickupTime,
            ScheduledReturnTime = booking.ScheduledReturnTime,
            Status = booking.Status.ToString(),
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt
        };
    }

    private BookingResponse MapToResponse(Booking booking, Vehicle vehicle)
    {
        return new BookingResponse
        {
            Id = booking.Id,
            BookingCode = booking.BookingCode,
            VehicleId = vehicle.Id,
            VehicleName = $"{vehicle.Brand} {vehicle.Model}",
            LicensePlate = vehicle.LicensePlate,
            StationId = booking.StationId,
            StationName = vehicle.Station?.Name ?? "",
            ScheduledPickupTime = booking.ScheduledPickupTime,
            ScheduledReturnTime = booking.ScheduledReturnTime,
            Status = booking.Status.ToString(),
            Notes = booking.Notes,
            CreatedAt = booking.CreatedAt
        };
    }
}

