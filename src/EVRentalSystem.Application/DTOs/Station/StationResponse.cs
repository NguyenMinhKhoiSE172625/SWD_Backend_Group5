namespace EVRentalSystem.Application.DTOs.Station;

public class StationResponse
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public int AvailableVehiclesCount { get; set; }
}

