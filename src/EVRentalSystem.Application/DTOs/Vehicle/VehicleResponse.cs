namespace EVRentalSystem.Application.DTOs.Vehicle;

public class VehicleResponse
{
    public int Id { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public string Brand { get; set; } = string.Empty;
    public int Year { get; set; }
    public string Color { get; set; } = string.Empty;
    public int BatteryCapacity { get; set; }
    public decimal PricePerHour { get; set; }
    public decimal PricePerDay { get; set; }
    public string Status { get; set; } = string.Empty;
    public string? ImageUrl { get; set; }
    public string? Description { get; set; }
    public int StationId { get; set; }
    public string StationName { get; set; } = string.Empty;
}

