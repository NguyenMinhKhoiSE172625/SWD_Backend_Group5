using EVRentalSystem.Application.DTOs.Admin;

namespace EVRentalSystem.Application.Interfaces;

public interface IAdminService
{
    Task<DashboardResponse> GetDashboardAsync();
    Task<List<RevenueReportResponse>> GetRevenueReportAsync(DateTime startDate, DateTime endDate);
    Task<List<VehicleUtilizationResponse>> GetVehicleUtilizationAsync();
    Task<UserReportResponse> GetUserReportAsync();
    Task<BookingAnalyticsResponse> GetBookingAnalyticsAsync();
    Task<List<VehicleUtilizationResponse>> GetPopularVehiclesAsync(int topCount = 10);
    Task<StaffResponse?> CreateStaffAsync(CreateStaffRequest request);
    Task<bool> DeleteUserAsync(int userId);
}

