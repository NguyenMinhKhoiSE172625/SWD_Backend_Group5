namespace EVRentalSystem.Application.DTOs.Admin;

public class RevenueReportResponse
{
    public DateTime Date { get; set; }
    public decimal Revenue { get; set; }
    public int BookingCount { get; set; }
    public int RentalCount { get; set; }
}

