namespace EVRentalSystem.Application.DTOs.Admin;

public class StaffResponse
{
    public int Id { get; set; }
    public string FullName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty;
    public int? StationId { get; set; }
    public string? StationName { get; set; }
    public DateTime CreatedAt { get; set; }
}
