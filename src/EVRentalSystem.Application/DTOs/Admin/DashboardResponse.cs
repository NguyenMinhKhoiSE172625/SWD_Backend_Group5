namespace EVRentalSystem.Application.DTOs.Admin;

public class DashboardResponse
{
    public decimal TotalRevenue { get; set; }
    public int TotalBookings { get; set; }
    public int TotalUsers { get; set; }
    public int TotalVehicles { get; set; }
    public int ActiveRentals { get; set; }
    public int AvailableVehicles { get; set; }
    public int PendingBookings { get; set; }
    public decimal TodayRevenue { get; set; }
    public int TodayBookings { get; set; }
}

