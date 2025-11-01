namespace EVRentalSystem.Application.DTOs.Booking;

public class BookingResponse
{
    public int Id { get; set; }
    public string BookingCode { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
    public DateTime ScheduledPickupTime { get; set; }
    public DateTime? ScheduledReturnTime { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? Notes { get; set; }
    public DateTime CreatedAt { get; set; }
}

