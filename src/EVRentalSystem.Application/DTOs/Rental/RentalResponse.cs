namespace EVRentalSystem.Application.DTOs.Rental;

public class RentalResponse
{
    public int Id { get; set; }
    public string RentalCode { get; set; } = string.Empty;
    public int VehicleId { get; set; }
    public string VehicleName { get; set; } = string.Empty;
    public string LicensePlate { get; set; } = string.Empty;
    public DateTime PickupTime { get; set; }
    public DateTime? ReturnTime { get; set; }
    public int PickupBatteryLevel { get; set; }
    public int? ReturnBatteryLevel { get; set; }
    public decimal? TotalDistance { get; set; }
    public decimal? TotalAmount { get; set; }
    public decimal? AdditionalFees { get; set; }
    public string? AdditionalFeesReason { get; set; }
    public string Status { get; set; } = string.Empty;
}

