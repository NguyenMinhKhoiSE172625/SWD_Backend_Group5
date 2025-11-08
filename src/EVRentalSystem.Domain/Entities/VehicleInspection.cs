namespace EVRentalSystem.Domain.Entities;

public class VehicleInspection
{
    public int Id { get; set; }
    
    public int VehicleId { get; set; }
    public Vehicle Vehicle { get; set; } = null!;
    
    public int? RentalId { get; set; }
    public Rental? Rental { get; set; }
    
    public int InspectorId { get; set; } // Staff user ID
    public User Inspector { get; set; } = null!;
    
    public bool IsPickup { get; set; } // true = pickup, false = return
    
    public string? ImageUrls { get; set; } // JSON array of image URLs
    public string? Notes { get; set; }
    public string? DamageReport { get; set; }
    
    // Odometer reading (km)
    public decimal? OdometerReading { get; set; }
    
    // Signatures (base64 encoded images)
    public string? RenterSignature { get; set; }
    public string? StaffSignature { get; set; }
    
    public DateTime InspectionDate { get; set; }
    public DateTime CreatedAt { get; set; }
}

