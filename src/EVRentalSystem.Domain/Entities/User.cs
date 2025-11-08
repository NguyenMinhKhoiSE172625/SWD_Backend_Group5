using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class User
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string PasswordHash { get; set; } = string.Empty;
    public UserRole Role { get; set; }
    
    // For Renters
    public string? DriverLicenseNumber { get; set; }
    public string? DriverLicenseImageUrl { get; set; }
    public string? IdCardNumber { get; set; }
    public string? IdCardImageUrl { get; set; }
    public bool IsVerified { get; set; }
    
    // For Staff
    public int? StationId { get; set; }
    public Station? Station { get; set; }
    
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Password reset
    public string? PasswordResetToken { get; set; }
    public DateTime? PasswordResetTokenExpiry { get; set; }
    
    // Navigation properties
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
    public ICollection<Rental> Rentals { get; set; } = new List<Rental>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
}

