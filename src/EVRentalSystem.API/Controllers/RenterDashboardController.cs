using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/renters/dashboard")]
[Authorize(Roles = "Renter")]
[ValidateModel]
public class RenterDashboardController : ControllerBase
{
    private readonly ApplicationDbContext _context;
    private readonly IRentalService _rentalService;
    private readonly IBookingService _bookingService;
    private readonly IPaymentService _paymentService;

    public RenterDashboardController(
        ApplicationDbContext context,
        IRentalService rentalService,
        IBookingService bookingService,
        IPaymentService paymentService)
    {
        _context = context;
        _rentalService = rentalService;
        _bookingService = bookingService;
        _paymentService = paymentService;
    }

    /// <summary>
    /// Lấy dữ liệu dashboard cho người thuê xe
    /// </summary>
    [HttpGet]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // Get user rentals
        var rentals = await _rentalService.GetUserRentalsAsync(userId);
        var completedRentals = rentals.Where(r => r.Status == "Completed").ToList();

        // Calculate statistics
        var totalTrips = completedRentals.Count;
        var totalDistance = completedRentals.Sum(r => r.TotalDistance ?? 0);
        var totalSpent = completedRentals.Sum(r => r.TotalAmount ?? 0);

        // Get recent rentals (last 5)
        var recentRentals = rentals.Take(5).ToList();

        // Get upcoming bookings
        var bookings = await _bookingService.GetUserBookingsAsync(userId);
        var upcomingBookings = bookings
            .Where(b => b.Status == "Confirmed" || b.Status == "Pending")
            .OrderBy(b => b.ScheduledPickupTime)
            .Take(5)
            .ToList();

        // Get payment history (last 5)
        var payments = await _paymentService.GetUserPaymentsAsync(userId);
        var recentPayments = payments.Take(5).ToList();

        // Analyze peak hours (rental pickup times)
        var peakHours = rentals
            .GroupBy(r => r.PickupTime.Hour)
            .OrderByDescending(g => g.Count())
            .Take(3)
            .Select(g => new { Hour = g.Key, Count = g.Count() })
            .ToList();

        var dashboard = new
        {
            Statistics = new
            {
                TotalTrips = totalTrips,
                TotalDistance = totalDistance,
                TotalSpent = totalSpent,
                AverageTripDistance = totalTrips > 0 ? totalDistance / totalTrips : 0,
                AverageTripCost = totalTrips > 0 ? totalSpent / totalTrips : 0
            },
            RecentRentals = recentRentals,
            UpcomingBookings = upcomingBookings,
            RecentPayments = recentPayments,
            PeakHours = peakHours
        };

        return Ok(ApiResponse<object>.SuccessResponse(dashboard));
    }

    /// <summary>
    /// Lấy thống kê cá nhân chi tiết
    /// </summary>
    [HttpGet("statistics")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> GetStatistics([FromQuery] DateTime? startDate = null, [FromQuery] DateTime? endDate = null)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        var rentals = await _rentalService.GetUserRentalsAsync(userId);
        var completedRentals = rentals.Where(r => r.Status == "Completed").ToList();

        if (startDate.HasValue)
        {
            completedRentals = completedRentals
                .Where(r => r.PickupTime >= startDate.Value)
                .ToList();
        }

        if (endDate.HasValue)
        {
            completedRentals = completedRentals
                .Where(r => r.PickupTime <= endDate.Value)
                .ToList();
        }

        // Calculate statistics
        var totalTrips = completedRentals.Count;
        var totalDistance = completedRentals.Sum(r => r.TotalDistance ?? 0);
        var totalSpent = completedRentals.Sum(r => r.TotalAmount ?? 0);

        // Peak hours analysis
        var hourlyStats = rentals
            .GroupBy(r => r.PickupTime.Hour)
            .Select(g => new
            {
                Hour = g.Key,
                Count = g.Count(),
                TotalDistance = g.Sum(r => r.TotalDistance ?? 0),
                TotalSpent = g.Sum(r => r.TotalAmount ?? 0)
            })
            .OrderByDescending(x => x.Count)
            .ToList();

        // Daily stats (last 30 days)
        var dailyStats = rentals
            .GroupBy(r => r.PickupTime.Date)
            .Select(g => new
            {
                Date = g.Key,
                Count = g.Count(),
                TotalDistance = g.Sum(r => r.TotalDistance ?? 0),
                TotalSpent = g.Sum(r => r.TotalAmount ?? 0)
            })
            .OrderByDescending(x => x.Date)
            .Take(30)
            .ToList();

        var statistics = new
        {
            Summary = new
            {
                TotalTrips = totalTrips,
                TotalDistance = totalDistance,
                TotalSpent = totalSpent,
                AverageTripDistance = totalTrips > 0 ? totalDistance / totalTrips : 0,
                AverageTripCost = totalTrips > 0 ? totalSpent / totalTrips : 0
            },
            HourlyStats = hourlyStats,
            DailyStats = dailyStats
        };

        return Ok(ApiResponse<object>.SuccessResponse(statistics));
    }
}

