namespace EVRentalSystem.Application.DTOs.Admin;

public class BookingAnalyticsResponse
{
    public int TotalBookings { get; set; }
    public int PendingBookings { get; set; }
    public int ConfirmedBookings { get; set; }
    public int CancelledBookings { get; set; }
    public int CompletedBookings { get; set; }
    public decimal CancellationRate { get; set; } // Percentage
    public decimal CompletionRate { get; set; } // Percentage
}

