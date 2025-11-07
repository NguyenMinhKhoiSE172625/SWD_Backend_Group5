using EVRentalSystem.Application.DTOs.Maintenance;

namespace EVRentalSystem.Application.Interfaces;

public interface IMaintenanceService
{
    Task<MaintenanceScheduleResponse?> CreateScheduleAsync(int staffId, CreateMaintenanceScheduleRequest request);
    Task<MaintenanceScheduleResponse?> UpdateScheduleAsync(int staffId, UpdateMaintenanceScheduleRequest request);
    Task<List<MaintenanceScheduleResponse>> GetVehicleSchedulesAsync(int vehicleId);
    Task<List<MaintenanceScheduleResponse>> GetUpcomingSchedulesAsync(int? stationId = null);
    
    Task<MaintenanceRecordResponse?> CreateRecordAsync(int technicianId, CreateMaintenanceRecordRequest request);
    Task<List<MaintenanceRecordResponse>> GetVehicleRecordsAsync(int vehicleId);
}

