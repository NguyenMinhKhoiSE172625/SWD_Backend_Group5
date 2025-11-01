using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class Booking
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public int StationId { get; set; }
    public Station Station { get; set; } = null!;
    
    public DateTime BookingDate { get; set; }
    public DateTime ScheduledPickupTime { get; set; }
    public DateTime? ScheduledReturnTime { get; set; }
    
    public BookingStatus Status { get; set; }
    public string? Notes { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public Rental? Rental { get; set; }
}

