using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class Rental
{
    public int Id { get; set; }
    public string RentalCode { get; set; } = string.Empty;
    
    public int? BookingId { get; set; }
    public Booking? Booking { get; set; }
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public DateTime PickupTime { get; set; }
    public DateTime? ReturnTime { get; set; }
    
    public int PickupBatteryLevel { get; set; }
    public int? ReturnBatteryLevel { get; set; }
    
    public decimal? TotalDistance { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AdditionalFees { get; set; }
    public string? AdditionalFeesReason { get; set; }
    
    public RentalStatus Status { get; set; }
    
    public int? PickupStaffId { get; set; }
    public int? ReturnStaffId { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<VehicleInspection> Inspections { get; set; } = new List<VehicleInspection>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

