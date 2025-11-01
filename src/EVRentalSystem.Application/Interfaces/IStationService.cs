using EVRentalSystem.Application.DTOs.Station;

namespace EVRentalSystem.Application.Interfaces;

public interface IStationService
{
    Task<List<StationResponse>> GetAllStationsAsync();
    Task<StationResponse?> GetStationByIdAsync(int stationId);
    Task<List<StationResponse>> GetNearbyStationsAsync(decimal latitude, decimal longitude, double radiusKm = 10);
}

