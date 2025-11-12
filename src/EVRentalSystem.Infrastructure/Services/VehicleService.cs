using EVRentalSystem.Application.DTOs.Vehicle;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class VehicleService : IVehicleService
{
    private readonly ApplicationDbContext _context;

    public VehicleService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<VehicleResponse>> GetAllVehiclesAsync(int? stationId = null, string? status = null)
    {
        var query = _context.Vehicles
            .Include(v => v.Station)
            .AsQueryable();

        if (stationId.HasValue)
        {
            query = query.Where(v => v.StationId == stationId.Value);
        }

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<VehicleStatus>(status, out var vehicleStatus))
        {
            query = query.Where(v => v.Status == vehicleStatus);
        }

        var vehicles = await query.OrderBy(v => v.LicensePlate).ToListAsync();
        return vehicles.Select(MapToResponse).ToList();
    }

    public async Task<List<VehicleResponse>> GetAvailableVehiclesAsync(int? stationId = null)
    {
        var query = _context.Vehicles
            .Include(v => v.Station)
            .Where(v => v.Status == VehicleStatus.Available);

        if (stationId.HasValue)
        {
            query = query.Where(v => v.StationId == stationId.Value);
        }

        var vehicles = await query.ToListAsync();
        return vehicles.Select(MapToResponse).ToList();
    }

    public async Task<VehicleResponse?> GetVehicleByIdAsync(int vehicleId)
    {
        var vehicle = await _context.Vehicles
            .Include(v => v.Station)
            .FirstOrDefaultAsync(v => v.Id == vehicleId);

        return vehicle == null ? null : MapToResponse(vehicle);
    }

    public async Task<List<VehicleResponse>> GetStationVehiclesAsync(int stationId, string? status = null)
    {
        var query = _context.Vehicles
            .Include(v => v.Station)
            .Where(v => v.StationId == stationId);

        if (!string.IsNullOrEmpty(status) && Enum.TryParse<VehicleStatus>(status, out var vehicleStatus))
        {
            query = query.Where(v => v.Status == vehicleStatus);
        }

        var vehicles = await query.ToListAsync();

        return vehicles.Select(MapToResponse).ToList();
    }

    public async Task<bool> UpdateVehicleStatusAsync(int vehicleId, string status, int staffId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle == null)
        {
            return false;
        }

        if (Enum.TryParse<VehicleStatus>(status, out var vehicleStatus))
        {
            vehicle.Status = vehicleStatus;
            vehicle.UpdatedAt = DateTime.UtcNow;
            await _context.SaveChangesAsync();
            return true;
        }

        return false;
    }

    public async Task<bool> UpdateVehicleBatteryAsync(int vehicleId, int batteryLevel, int staffId)
    {
        var vehicle = await _context.Vehicles.FindAsync(vehicleId);
        if (vehicle == null || batteryLevel < 0 || batteryLevel > 100)
        {
            return false;
        }

        vehicle.BatteryCapacity = batteryLevel;
        vehicle.UpdatedAt = DateTime.UtcNow;
        await _context.SaveChangesAsync();
        return true;
    }

    private VehicleResponse MapToResponse(Domain.Entities.Vehicle vehicle)
    {
        return new VehicleResponse
        {
            Id = vehicle.Id,
            LicensePlate = vehicle.LicensePlate,
            Model = vehicle.Model,
            Brand = vehicle.Brand,
            Year = vehicle.Year,
            Color = vehicle.Color,
            BatteryCapacity = vehicle.BatteryCapacity,
            PricePerHour = vehicle.PricePerHour,
            PricePerDay = vehicle.PricePerDay,
            Status = vehicle.Status.ToString(),
            ImageUrl = vehicle.ImageUrl,
            Description = vehicle.Description,
            StationId = vehicle.StationId,
            StationName = vehicle.Station?.Name ?? ""
        };
    }
}

