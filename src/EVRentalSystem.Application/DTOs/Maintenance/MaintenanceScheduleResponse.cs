namespace EVRentalSystem.Application.DTOs.Maintenance;

public class MaintenanceScheduleResponse
{
    public int Id { get; set; }
    public int VehicleId { get; set; }
    public string VehicleLicensePlate { get; set; } = string.Empty;
    public DateTime ScheduledDate { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public int? AssignedTechnicianId { get; set; }
    public string? AssignedTechnicianName { get; set; }
    public DateTime CreatedAt { get; set; }
}

