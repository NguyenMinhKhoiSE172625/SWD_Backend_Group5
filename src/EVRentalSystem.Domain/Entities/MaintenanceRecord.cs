using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class MaintenanceRecord
{
    public int Id { get; set; }
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public int? MaintenanceScheduleId { get; set; }
    public MaintenanceSchedule? MaintenanceSchedule { get; set; }
    
    public DateTime MaintenanceDate { get; set; }
    public MaintenanceType Type { get; set; }
    
    public decimal Cost { get; set; }
    public string Description { get; set; } = string.Empty;
    
    public int TechnicianId { get; set; } // Staff user ID
    
    public DateTime CreatedAt { get; set; }
}

