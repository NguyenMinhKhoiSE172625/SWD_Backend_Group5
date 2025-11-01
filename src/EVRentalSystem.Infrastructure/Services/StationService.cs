using EVRentalSystem.Application.DTOs.Station;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class StationService : IStationService
{
    private readonly ApplicationDbContext _context;

    public StationService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<List<StationResponse>> GetAllStationsAsync()
    {
        var stations = await _context.Stations
            .Include(s => s.Vehicles)
            .Where(s => s.IsActive)
            .ToListAsync();

        return stations.Select(MapToResponse).ToList();
    }

    public async Task<StationResponse?> GetStationByIdAsync(int stationId)
    {
        var station = await _context.Stations
            .Include(s => s.Vehicles)
            .FirstOrDefaultAsync(s => s.Id == stationId);

        return station == null ? null : MapToResponse(station);
    }

    public async Task<List<StationResponse>> GetNearbyStationsAsync(decimal latitude, decimal longitude, double radiusKm = 10)
    {
        var stations = await _context.Stations
            .Include(s => s.Vehicles)
            .Where(s => s.IsActive)
            .ToListAsync();

        // Simple distance calculation (Haversine formula)
        var nearbyStations = stations
            .Select(s => new
            {
                Station = s,
                Distance = CalculateDistance((double)latitude, (double)longitude, (double)s.Latitude, (double)s.Longitude)
            })
            .Where(x => x.Distance <= radiusKm)
            .OrderBy(x => x.Distance)
            .Select(x => MapToResponse(x.Station))
            .ToList();

        return nearbyStations;
    }

    private double CalculateDistance(double lat1, double lon1, double lat2, double lon2)
    {
        const double R = 6371; // Earth's radius in kilometers
        var dLat = ToRadians(lat2 - lat1);
        var dLon = ToRadians(lon2 - lon1);
        
        var a = Math.Sin(dLat / 2) * Math.Sin(dLat / 2) +
                Math.Cos(ToRadians(lat1)) * Math.Cos(ToRadians(lat2)) *
                Math.Sin(dLon / 2) * Math.Sin(dLon / 2);
        
        var c = 2 * Math.Atan2(Math.Sqrt(a), Math.Sqrt(1 - a));
        return R * c;
    }

    private double ToRadians(double degrees)
    {
        return degrees * Math.PI / 180;
    }

    private StationResponse MapToResponse(Domain.Entities.Station station)
    {
        return new StationResponse
        {
            Id = station.Id,
            Name = station.Name,
            Address = station.Address,
            Latitude = station.Latitude,
            Longitude = station.Longitude,
            PhoneNumber = station.PhoneNumber,
            Description = station.Description,
            IsActive = station.IsActive,
            AvailableVehiclesCount = station.Vehicles.Count(v => v.Status == VehicleStatus.Available)
        };
    }
}

