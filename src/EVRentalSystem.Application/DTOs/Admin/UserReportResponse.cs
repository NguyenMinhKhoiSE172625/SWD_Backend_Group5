namespace EVRentalSystem.Application.DTOs.Admin;

public class UserReportResponse
{
    public int TotalUsers { get; set; }
    public int VerifiedUsers { get; set; }
    public int UnverifiedUsers { get; set; }
    public int Renters { get; set; }
    public int StationStaff { get; set; }
    public int Admins { get; set; }
    public int NewUsersThisMonth { get; set; }
}

