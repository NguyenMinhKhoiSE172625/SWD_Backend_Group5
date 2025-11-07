using EVRentalSystem.Application.DTOs.Admin;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class AdminService : IAdminService
{
    private readonly ApplicationDbContext _context;

    public AdminService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<DashboardResponse> GetDashboardAsync()
    {
        var today = DateTime.UtcNow.Date;

        var totalRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var todayRevenue = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed && p.PaymentDate.Date == today)
            .SumAsync(p => (decimal?)p.Amount) ?? 0;

        var totalBookings = await _context.Bookings.CountAsync();
        var todayBookings = await _context.Bookings.CountAsync(b => b.BookingDate.Date == today);
        var totalUsers = await _context.Users.CountAsync();
        var totalVehicles = await _context.Vehicles.CountAsync();
        var activeRentals = await _context.Rentals.CountAsync(r => r.Status == RentalStatus.Active);
        var availableVehicles = await _context.Vehicles.CountAsync(v => v.Status == VehicleStatus.Available);
        var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);

        return new DashboardResponse
        {
            TotalRevenue = totalRevenue,
            TotalBookings = totalBookings,
            TotalUsers = totalUsers,
            TotalVehicles = totalVehicles,
            ActiveRentals = activeRentals,
            AvailableVehicles = availableVehicles,
            PendingBookings = pendingBookings,
            TodayRevenue = todayRevenue,
            TodayBookings = todayBookings
        };
    }

    public async Task<List<RevenueReportResponse>> GetRevenueReportAsync(DateTime startDate, DateTime endDate)
    {
        var payments = await _context.Payments
            .Where(p => p.Status == PaymentStatus.Completed 
                && p.PaymentDate >= startDate 
                && p.PaymentDate <= endDate)
            .GroupBy(p => p.PaymentDate.Date)
            .Select(g => new RevenueReportResponse
            {
                Date = g.Key,
                Revenue = g.Sum(p => p.Amount),
                BookingCount = g.Count(),
                RentalCount = g.Count(p => p.RentalId != null)
            })
            .OrderBy(r => r.Date)
            .ToListAsync();

        return payments;
    }

    public async Task<List<VehicleUtilizationResponse>> GetVehicleUtilizationAsync()
    {
        var vehicles = await _context.Vehicles
            .Include(v => v.Rentals)
            .ToListAsync();

        var utilizationList = new List<VehicleUtilizationResponse>();

        foreach (var vehicle in vehicles)
        {
            var totalRentals = vehicle.Rentals.Count;
            var totalRevenue = vehicle.Rentals
                .Where(r => r.TotalAmount.HasValue)
                .Sum(r => r.TotalAmount ?? 0);

            // Calculate utilization rate (simplified: rentals / days since creation)
            var daysSinceCreation = (DateTime.UtcNow - vehicle.CreatedAt).TotalDays;
            var utilizationRate = daysSinceCreation > 0 
                ? (decimal)(totalRentals / daysSinceCreation * 100) 
                : 0;

            utilizationList.Add(new VehicleUtilizationResponse
            {
                VehicleId = vehicle.Id,
                LicensePlate = vehicle.LicensePlate,
                Model = vehicle.Model,
                UtilizationRate = Math.Round(utilizationRate, 2),
                TotalRentals = totalRentals,
                TotalRevenue = totalRevenue,
                Status = vehicle.Status.ToString()
            });
        }

        return utilizationList.OrderByDescending(v => v.UtilizationRate).ToList();
    }

    public async Task<UserReportResponse> GetUserReportAsync()
    {
        var totalUsers = await _context.Users.CountAsync();
        var verifiedUsers = await _context.Users.CountAsync(u => u.IsVerified);
        var unverifiedUsers = totalUsers - verifiedUsers;
        var renters = await _context.Users.CountAsync(u => u.Role == UserRole.Renter);
        var staff = await _context.Users.CountAsync(u => u.Role == UserRole.StationStaff);
        var admins = await _context.Users.CountAsync(u => u.Role == UserRole.Admin);

        var firstDayOfMonth = new DateTime(DateTime.UtcNow.Year, DateTime.UtcNow.Month, 1);
        var newUsersThisMonth = await _context.Users.CountAsync(u => u.CreatedAt >= firstDayOfMonth);

        return new UserReportResponse
        {
            TotalUsers = totalUsers,
            VerifiedUsers = verifiedUsers,
            UnverifiedUsers = unverifiedUsers,
            Renters = renters,
            StationStaff = staff,
            Admins = admins,
            NewUsersThisMonth = newUsersThisMonth
        };
    }

    public async Task<BookingAnalyticsResponse> GetBookingAnalyticsAsync()
    {
        var totalBookings = await _context.Bookings.CountAsync();
        var pendingBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Pending);
        var confirmedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Confirmed);
        var cancelledBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Cancelled);
        var completedBookings = await _context.Bookings.CountAsync(b => b.Status == BookingStatus.Completed);

        var cancellationRate = totalBookings > 0 
            ? (decimal)cancelledBookings / totalBookings * 100 
            : 0;

        var completionRate = totalBookings > 0 
            ? (decimal)completedBookings / totalBookings * 100 
            : 0;

        return new BookingAnalyticsResponse
        {
            TotalBookings = totalBookings,
            PendingBookings = pendingBookings,
            ConfirmedBookings = confirmedBookings,
            CancelledBookings = cancelledBookings,
            CompletedBookings = completedBookings,
            CancellationRate = Math.Round(cancellationRate, 2),
            CompletionRate = Math.Round(completionRate, 2)
        };
    }

    public async Task<List<VehicleUtilizationResponse>> GetPopularVehiclesAsync(int topCount = 10)
    {
        var vehicles = await _context.Vehicles
            .Include(v => v.Rentals)
            .ToListAsync();

        var popularVehicles = vehicles
            .Select(v => new VehicleUtilizationResponse
            {
                VehicleId = v.Id,
                LicensePlate = v.LicensePlate,
                Model = v.Model,
                TotalRentals = v.Rentals.Count,
                TotalRevenue = v.Rentals.Where(r => r.TotalAmount.HasValue).Sum(r => r.TotalAmount ?? 0),
                Status = v.Status.ToString(),
                UtilizationRate = 0 // Not needed for this report
            })
            .OrderByDescending(v => v.TotalRentals)
            .Take(topCount)
            .ToList();

        return popularVehicles;
    }
}

