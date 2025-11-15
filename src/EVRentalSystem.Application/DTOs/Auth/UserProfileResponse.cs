namespace EVRentalSystem.Application.DTOs.Auth;

public class UserProfileResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public string? DriverLicenseNumber { get; set; }
    public string? IdCardNumber { get; set; }
    public bool IsVerified { get; set; }
    public int? StationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
