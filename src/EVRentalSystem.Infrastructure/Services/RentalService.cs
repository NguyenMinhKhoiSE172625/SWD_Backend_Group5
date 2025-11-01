using System.Text.Json;
using EVRentalSystem.Application.DTOs.Rental;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class RentalService : IRentalService
{
    private readonly ApplicationDbContext _context;

    public RentalService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<RentalResponse?> CreateRentalAsync(int userId, int staffId, CreateRentalRequest request)
    {
        var vehicle = await _context.Vehicles.FindAsync(request.VehicleId);
        if (vehicle == null)
        {
            return null;
        }

        Booking? booking = null;
        if (request.BookingId.HasValue)
        {
            booking = await _context.Bookings.FindAsync(request.BookingId.Value);
            if (booking == null || booking.UserId != userId || booking.Status != BookingStatus.Confirmed)
            {
                return null;
            }
        }

        var rental = new Rental
        {
            RentalCode = GenerateRentalCode(),
            BookingId = request.BookingId,
            UserId = userId,
            VehicleId = request.VehicleId,
            PickupTime = DateTime.UtcNow,
            PickupBatteryLevel = request.PickupBatteryLevel,
            Status = RentalStatus.Active,
            PickupStaffId = staffId,
            CreatedAt = DateTime.UtcNow
        };

        _context.Rentals.Add(rental);

        // Create pickup inspection
        var inspection = new VehicleInspection
        {
            VehicleId = request.VehicleId,
            RentalId = rental.Id,
            InspectorId = staffId,
            IsPickup = true,
            ImageUrls = request.PickupImageUrls != null ? JsonSerializer.Serialize(request.PickupImageUrls) : null,
            Notes = request.PickupNotes,
            InspectionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.VehicleInspections.Add(inspection);

        // Update vehicle status
        vehicle.Status = VehicleStatus.InUse;
        vehicle.BatteryCapacity = request.PickupBatteryLevel;
        vehicle.UpdatedAt = DateTime.UtcNow;

        // Update booking if exists
        if (booking != null)
        {
            booking.Status = BookingStatus.Completed;
            booking.UpdatedAt = DateTime.UtcNow;
        }

        await _context.SaveChangesAsync();

        return MapToResponse(rental, vehicle);
    }

    public async Task<RentalResponse?> CompleteRentalAsync(int staffId, CompleteRentalRequest request)
    {
        var rental = await _context.Rentals
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == request.RentalId);

        if (rental == null || rental.Status != RentalStatus.Active)
        {
            return null;
        }

        rental.ReturnTime = DateTime.UtcNow;
        rental.ReturnBatteryLevel = request.ReturnBatteryLevel;
        rental.TotalDistance = request.TotalDistance;
        rental.AdditionalFees = request.AdditionalFees;
        rental.AdditionalFeesReason = request.AdditionalFeesReason;
        rental.Status = RentalStatus.Completed;
        rental.ReturnStaffId = staffId;
        rental.UpdatedAt = DateTime.UtcNow;

        // Calculate total amount
        var duration = rental.ReturnTime.Value - rental.PickupTime;
        var hours = (decimal)duration.TotalHours;
        
        if (hours <= 24)
        {
            rental.TotalAmount = Math.Ceiling(hours) * rental.Vehicle.PricePerHour;
        }
        else
        {
            var days = Math.Ceiling((decimal)duration.TotalDays);
            rental.TotalAmount = days * rental.Vehicle.PricePerDay;
        }

        if (rental.AdditionalFees.HasValue)
        {
            rental.TotalAmount += rental.AdditionalFees.Value;
        }

        // Create return inspection
        var inspection = new VehicleInspection
        {
            VehicleId = rental.VehicleId,
            RentalId = rental.Id,
            InspectorId = staffId,
            IsPickup = false,
            ImageUrls = request.ReturnImageUrls != null ? JsonSerializer.Serialize(request.ReturnImageUrls) : null,
            Notes = request.ReturnNotes,
            DamageReport = request.DamageReport,
            InspectionDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.VehicleInspections.Add(inspection);

        // Update vehicle status
        if (!string.IsNullOrEmpty(request.DamageReport))
        {
            rental.Vehicle.Status = VehicleStatus.Damaged;
        }
        else
        {
            rental.Vehicle.Status = VehicleStatus.Available;
        }
        
        rental.Vehicle.BatteryCapacity = request.ReturnBatteryLevel;
        rental.Vehicle.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return MapToResponse(rental, rental.Vehicle);
    }

    public async Task<RentalResponse?> GetRentalByIdAsync(int rentalId)
    {
        var rental = await _context.Rentals
            .Include(r => r.Vehicle)
            .FirstOrDefaultAsync(r => r.Id == rentalId);

        return rental == null ? null : MapToResponse(rental, rental.Vehicle);
    }

    public async Task<List<RentalResponse>> GetUserRentalsAsync(int userId)
    {
        var rentals = await _context.Rentals
            .Include(r => r.Vehicle)
            .Where(r => r.UserId == userId)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return rentals.Select(r => MapToResponse(r, r.Vehicle)).ToList();
    }

    public async Task<List<RentalResponse>> GetActiveRentalsAsync()
    {
        var rentals = await _context.Rentals
            .Include(r => r.Vehicle)
            .Where(r => r.Status == RentalStatus.Active)
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return rentals.Select(r => MapToResponse(r, r.Vehicle)).ToList();
    }

    private string GenerateRentalCode()
    {
        return $"RN{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
    }

    private RentalResponse MapToResponse(Rental rental, Vehicle vehicle)
    {
        return new RentalResponse
        {
            Id = rental.Id,
            RentalCode = rental.RentalCode,
            VehicleId = vehicle.Id,
            VehicleName = $"{vehicle.Brand} {vehicle.Model}",
            LicensePlate = vehicle.LicensePlate,
            PickupTime = rental.PickupTime,
            ReturnTime = rental.ReturnTime,
            PickupBatteryLevel = rental.PickupBatteryLevel,
            ReturnBatteryLevel = rental.ReturnBatteryLevel,
            TotalDistance = rental.TotalDistance,
            TotalAmount = rental.TotalAmount,
            AdditionalFees = rental.AdditionalFees,
            AdditionalFeesReason = rental.AdditionalFeesReason,
            Status = rental.Status.ToString()
        };
    }
}

