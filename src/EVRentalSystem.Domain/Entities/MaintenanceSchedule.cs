using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class MaintenanceSchedule
{
    public int Id { get; set; }
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public DateTime ScheduledDate { get; set; }
    public MaintenanceType Type { get; set; }
    public MaintenanceStatus Status { get; set; }
    
    public string? Notes { get; set; }
    
    public int? AssignedTechnicianId { get; set; } // Staff user ID
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

