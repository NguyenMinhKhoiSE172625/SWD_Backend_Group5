namespace EVRentalSystem.Application.DTOs.Admin;

public class VehicleUtilizationResponse
{
    public int VehicleId { get; set; }
    public string LicensePlate { get; set; } = string.Empty;
    public string Model { get; set; } = string.Empty;
    public decimal UtilizationRate { get; set; } // Percentage
    public int TotalRentals { get; set; }
    public decimal TotalRevenue { get; set; }
    public string Status { get; set; } = string.Empty;
}

