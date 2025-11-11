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
            if (booking == null || booking.Status != BookingStatus.Confirmed)
            {
                return null;
            }
            // Use userId from booking if provided
            userId = booking.UserId;
        }
        else if (userId == 0)
        {
            // UserId is required for walk-in customers
            return null;
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
            Rental = rental, // Use navigation property instead of RentalId
            InspectorId = staffId,
            IsPickup = true,
            ImageUrls = request.PickupImageUrls != null ? JsonSerializer.Serialize(request.PickupImageUrls) : null,
            Notes = request.PickupNotes,
            OdometerReading = request.OdometerBeforePickup,
            RenterSignature = request.RenterSignature,
            StaffSignature = request.StaffSignature,
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
            Rental = rental, // Use navigation property
            InspectorId = staffId,
            IsPickup = false,
            ImageUrls = request.ReturnImageUrls != null ? JsonSerializer.Serialize(request.ReturnImageUrls) : null,
            Notes = request.ReturnNotes,
            DamageReport = request.DamageReport,
            OdometerReading = request.OdometerAfterReturn,
            RenterSignature = request.RenterSignature,
            StaffSignature = request.StaffSignature,
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

    public async Task<List<RentalResponse>> GetActiveRentalsAsync(int? stationId = null)
    {
        var query = _context.Rentals
            .Include(r => r.Vehicle)
            .Where(r => r.Status == RentalStatus.Active);

        if (stationId.HasValue)
        {
            query = query.Where(r => r.Vehicle.StationId == stationId.Value);
        }

        var rentals = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return rentals.Select(r => MapToResponse(r, r.Vehicle)).ToList();
    }

    public async Task<List<object>> GetRentalInspectionsAsync(int rentalId)
    {
        var inspections = await _context.VehicleInspections
            .Where(i => i.RentalId == rentalId)
            .OrderBy(i => i.InspectionDate)
            .ToListAsync();

        return inspections.Select(i => new
        {
            Id = i.Id,
            IsPickup = i.IsPickup,
            ImageUrls = string.IsNullOrEmpty(i.ImageUrls) 
                ? new List<string>() 
                : JsonSerializer.Deserialize<List<string>>(i.ImageUrls) ?? new List<string>(),
            Notes = i.Notes,
            DamageReport = i.DamageReport,
            InspectionDate = i.InspectionDate,
            InspectorId = i.InspectorId
        }).Cast<object>().ToList();
    }

    public async Task<List<RentalResponse>> GetStationRentalsAsync(int stationId, string? status = null)
    {
        var query = _context.Rentals
            .Include(r => r.Vehicle)
            .Where(r => r.Vehicle.StationId == stationId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<RentalStatus>(status, out var rentalStatus))
        {
            query = query.Where(r => r.Status == rentalStatus);
        }

        var rentals = await query
            .OrderByDescending(r => r.CreatedAt)
            .ToListAsync();

        return rentals.Select(r => MapToResponse(r, r.Vehicle)).ToList();
    }

    public async Task<object?> GetRentalForCheckinAsync(int rentalId)
    {
        var rental = await _context.Rentals
            .Include(r => r.User)
            .Include(r => r.Vehicle)
                .ThenInclude(v => v.Station)
            .Include(r => r.Inspections)
            .FirstOrDefaultAsync(r => r.Id == rentalId);

        if (rental == null)
        {
            return null;
        }

        // Get pickup inspection
        var pickupInspection = rental.Inspections
            .Where(i => i.IsPickup)
            .FirstOrDefault();

        return new
        {
            Id = rental.Id,
            RentalCode = rental.RentalCode,
            Vehicle = new
            {
                Id = rental.Vehicle.Id,
                LicensePlate = rental.Vehicle.LicensePlate,
                Model = rental.Vehicle.Model,
                Brand = rental.Vehicle.Brand,
                Year = rental.Vehicle.Year,
                Color = rental.Vehicle.Color,
                ImageUrl = rental.Vehicle.ImageUrl,
                Description = rental.Vehicle.Description,
                VehicleName = $"{rental.Vehicle.Brand} {rental.Vehicle.Model}"
            },
            Station = new
            {
                Id = rental.Vehicle.StationId,
                Name = rental.Vehicle.Station?.Name ?? ""
            },
            Customer = new
            {
                Id = rental.User.Id,
                FullName = rental.User.FullName,
                Email = rental.User.Email,
                PhoneNumber = rental.User.PhoneNumber
            },
            PickupTime = rental.PickupTime,
            ReturnTime = rental.ReturnTime,
            PickupBatteryLevel = rental.PickupBatteryLevel,
            ReturnBatteryLevel = rental.ReturnBatteryLevel,
            TotalDistance = rental.TotalDistance,
            Status = rental.Status.ToString(),
            // Pickup inspection info
            PickupInspection = pickupInspection != null ? new
            {
                OdometerReading = pickupInspection.OdometerReading,
                Notes = pickupInspection.Notes
            } : null
        };
    }

    public async Task<object> GetRentalHistoryAsync(int? stationId, string? type, DateTime? fromDate, DateTime? toDate, string? search, int page, int pageSize)
    {
        // Get all inspections (checkout = pickup, checkin = return)
        var query = _context.VehicleInspections
            .Include(i => i.Rental)
                .ThenInclude(r => r.Vehicle)
                    .ThenInclude(v => v.Station)
            .Include(i => i.Rental)
                .ThenInclude(r => r.User)
            .Include(i => i.Inspector)
            .AsQueryable();

        // Filter by station
        if (stationId.HasValue)
        {
            query = query.Where(i => i.Rental.Vehicle.StationId == stationId.Value);
        }

        // Filter by type (checkout = pickup, checkin = return)
        if (!string.IsNullOrEmpty(type))
        {
            if (type.ToLower() == "checkout" || type.ToLower() == "giao xe")
            {
                query = query.Where(i => i.IsPickup);
            }
            else if (type.ToLower() == "checkin" || type.ToLower() == "nhan xe")
            {
                query = query.Where(i => !i.IsPickup);
            }
        }

        // Filter by date range
        if (fromDate.HasValue)
        {
            query = query.Where(i => i.InspectionDate >= fromDate.Value);
        }

        if (toDate.HasValue)
        {
            var endDate = toDate.Value.Date.AddDays(1);
            query = query.Where(i => i.InspectionDate < endDate);
        }

        // Search
        if (!string.IsNullOrEmpty(search))
        {
            var searchLower = search.ToLower();
            query = query.Where(i => 
                i.Rental.RentalCode.ToLower().Contains(searchLower) ||
                i.Rental.Vehicle.LicensePlate.ToLower().Contains(searchLower) ||
                i.Rental.User.FullName.ToLower().Contains(searchLower));
        }

        var totalCount = await query.CountAsync();

        // Pagination
        var inspections = await query
            .OrderByDescending(i => i.InspectionDate)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        var items = inspections.Select(i => new
        {
            Id = i.Id,
            Type = i.IsPickup ? "Giao xe" : "Nhận xe",
            TypeCode = i.IsPickup ? "checkout" : "checkin",
            Time = i.InspectionDate,
            RentalCode = i.Rental.RentalCode,
            RentalId = i.RentalId,
            LicensePlate = i.Rental.Vehicle.LicensePlate,
            VehicleModel = $"{i.Rental.Vehicle.Brand} {i.Rental.Vehicle.Model}",
            CustomerName = i.Rental.User.FullName,
            StaffName = i.Inspector != null ? i.Inspector.FullName : "N/A",
            Odometer = i.OdometerReading,
            BatteryLevel = i.IsPickup ? i.Rental.PickupBatteryLevel : i.Rental.ReturnBatteryLevel,
            Notes = i.Notes,
            DamageReport = i.DamageReport
        }).ToList();

        return new
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
            TotalPages = (int)Math.Ceiling(totalCount / (double)pageSize)
        };
    }

    public async Task<object> GetRentalHistoryStatisticsAsync(int? stationId, DateTime? date)
    {
        var targetDate = date?.Date ?? DateTime.UtcNow.Date;
        var nextDate = targetDate.AddDays(1);

        var query = _context.VehicleInspections
            .Include(i => i.Rental)
                .ThenInclude(r => r.Vehicle)
            .Where(i => i.InspectionDate >= targetDate && i.InspectionDate < nextDate);

        if (stationId.HasValue)
        {
            query = query.Where(i => i.Rental.Vehicle.StationId == stationId.Value);
        }

        var inspections = await query.ToListAsync();

        var checkoutToday = inspections.Count(i => i.IsPickup);
        var checkinToday = inspections.Count(i => !i.IsPickup);
        var processedToday = inspections.Select(i => i.RentalId).Distinct().Count();

        return new
        {
            Date = targetDate,
            CheckoutToday = checkoutToday,
            CheckinToday = checkinToday,
            ProcessedToday = processedToday
        };
    }

    public async Task<object?> GetInspectionDetailAsync(int inspectionId)
    {
        var inspection = await _context.VehicleInspections
            .Include(i => i.Rental)
                .ThenInclude(r => r.Vehicle)
                    .ThenInclude(v => v.Station)
            .Include(i => i.Rental)
                .ThenInclude(r => r.User)
            .Include(i => i.Inspector)
            .FirstOrDefaultAsync(i => i.Id == inspectionId);

        if (inspection == null)
        {
            return null;
        }

        return new
        {
            Id = inspection.Id,
            Type = inspection.IsPickup ? "Giao xe" : "Nhận xe",
            TypeCode = inspection.IsPickup ? "checkout" : "checkin",
            InspectionDate = inspection.InspectionDate,
            Rental = new
            {
                Id = inspection.Rental.Id,
                RentalCode = inspection.Rental.RentalCode,
                PickupTime = inspection.Rental.PickupTime,
                ReturnTime = inspection.Rental.ReturnTime
            },
            Vehicle = new
            {
                Id = inspection.Vehicle.Id,
                LicensePlate = inspection.Vehicle.LicensePlate,
                Model = inspection.Vehicle.Model,
                Brand = inspection.Vehicle.Brand,
                VehicleName = $"{inspection.Vehicle.Brand} {inspection.Vehicle.Model}"
            },
            Customer = new
            {
                Id = inspection.Rental.User.Id,
                FullName = inspection.Rental.User.FullName,
                Email = inspection.Rental.User.Email,
                PhoneNumber = inspection.Rental.User.PhoneNumber
            },
            Staff = new
            {
                Id = inspection.InspectorId,
                Name = inspection.Inspector != null ? inspection.Inspector.FullName : "N/A"
            },
            OdometerReading = inspection.OdometerReading,
            BatteryLevel = inspection.IsPickup ? inspection.Rental.PickupBatteryLevel : inspection.Rental.ReturnBatteryLevel,
            ImageUrls = string.IsNullOrEmpty(inspection.ImageUrls)
                ? new List<string>()
                : JsonSerializer.Deserialize<List<string>>(inspection.ImageUrls) ?? new List<string>(),
            Notes = inspection.Notes,
            DamageReport = inspection.DamageReport,
            RenterSignature = inspection.RenterSignature,
            StaffSignature = inspection.StaffSignature
        };
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

