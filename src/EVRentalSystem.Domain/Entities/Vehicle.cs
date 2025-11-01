using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class Vehicle
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public int BatteryCapacity { get; set; } // in percentage
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public VehicleStatus Status { get; set; }
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    
    public int StationId { get; set; }
    public Station Station { get; set; } = null!;
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public ICollection<VehicleInspection> Inspections { get; set; } = new List<VehicleInspection>();
}

