using EVRentalSystem.Application.DTOs.Vehicle;

namespace EVRentalSystem.Application.Interfaces;

public interface IVehicleService
{
    Task<List<VehicleResponse>> GetAvailableVehiclesAsync(int? stationId = null);
    Task<VehicleResponse?> GetVehicleByIdAsync(int vehicleId);
    Task<List<VehicleResponse>> GetStationVehiclesAsync(int stationId);
    Task<bool> UpdateVehicleStatusAsync(int vehicleId, string status, int staffId);
    Task<bool> UpdateVehicleBatteryAsync(int vehicleId, int batteryLevel, int staffId);
}

