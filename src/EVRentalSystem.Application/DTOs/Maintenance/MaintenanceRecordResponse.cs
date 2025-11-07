namespace EVRentalSystem.Application.DTOs.Maintenance;

public class MaintenanceRecordResponse
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleLicensePlate { get; set; } = string.Empty;
    public int? MaintenanceScheduleId { get; set; }
    public DateTime MaintenanceDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public decimal Cost { get; set; }
    public string Description { get; set; } = string.Empty;
    public int TechnicianId { get; set; }
    public string TechnicianName { get; set; } = string.Empty;
    public DateTime CreatedAt { get; set; }
}

