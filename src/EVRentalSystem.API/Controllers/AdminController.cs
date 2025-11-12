using EVRentalSystem.Application.DTOs.Admin;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly IAdminService _adminService;

    public AdminController(IAdminService adminService)
    {
        _adminService = adminService;
    }

    /// <summary>
    /// Lấy dữ liệu dashboard tổng quan (Admin)
    /// </summary>
    [HttpGet("dashboard")]
    [ProducesResponseType(typeof(ApiResponse<DashboardResponse>), 200)]
    public async Task<IActionResult> GetDashboard()
    {
        var dashboard = await _adminService.GetDashboardAsync();
        return Ok(ApiResponse<DashboardResponse>.SuccessResponse(dashboard));
    }

    /// <summary>
    /// Báo cáo doanh thu theo khoảng thời gian (Admin)
    /// </summary>
    [HttpGet("reports/revenue")]
    [ProducesResponseType(typeof(ApiResponse<List<RevenueReportResponse>>), 200)]
    public async Task<IActionResult> GetRevenueReport([FromQuery] DateTime startDate, [FromQuery] DateTime endDate)
    {
        var report = await _adminService.GetRevenueReportAsync(startDate, endDate);
        return Ok(ApiResponse<List<RevenueReportResponse>>.SuccessResponse(report));
    }

    /// <summary>
    /// Báo cáo tình trạng và hiệu suất xe (Admin)
    /// </summary>
    [HttpGet("reports/vehicles")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleUtilizationResponse>>), 200)]
    public async Task<IActionResult> GetVehicleUtilization()
    {
        var report = await _adminService.GetVehicleUtilizationAsync();
        return Ok(ApiResponse<List<VehicleUtilizationResponse>>.SuccessResponse(report));
    }

    /// <summary>
    /// Báo cáo người dùng (Admin)
    /// </summary>
    [HttpGet("reports/users")]
    [ProducesResponseType(typeof(ApiResponse<UserReportResponse>), 200)]
    public async Task<IActionResult> GetUserReport()
    {
        var report = await _adminService.GetUserReportAsync();
        return Ok(ApiResponse<UserReportResponse>.SuccessResponse(report));
    }

    /// <summary>
    /// Phân tích đặt xe (Admin)
    /// </summary>
    [HttpGet("analytics/bookings")]
    [ProducesResponseType(typeof(ApiResponse<BookingAnalyticsResponse>), 200)]
    public async Task<IActionResult> GetBookingAnalytics()
    {
        var analytics = await _adminService.GetBookingAnalyticsAsync();
        return Ok(ApiResponse<BookingAnalyticsResponse>.SuccessResponse(analytics));
    }

    /// <summary>
    /// Danh sách xe được thuê nhiều nhất (Admin)
    /// </summary>
    [HttpGet("analytics/popular-vehicles")]
    [ProducesResponseType(typeof(ApiResponse<List<VehicleUtilizationResponse>>), 200)]
    public async Task<IActionResult> GetPopularVehicles([FromQuery] int topCount = 10)
    {
        var vehicles = await _adminService.GetPopularVehiclesAsync(topCount);
        return Ok(ApiResponse<List<VehicleUtilizationResponse>>.SuccessResponse(vehicles));
    }

    /// <summary>
    /// Seed bookings data (Admin only - Development)
    /// </summary>
    [HttpPost("seed/bookings")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> SeedBookings([FromServices] ApplicationDbContext context)
    {
        // Check if bookings already exist
        if (await context.Bookings.AnyAsync())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Bookings đã tồn tại. Xóa hết bookings trước khi seed."));
        }

        // Get existing users and vehicles
        var users = await context.Users.Where(u => u.Role == UserRole.Renter).ToListAsync();
        var vehicles = await context.Vehicles.ToListAsync();
        var stations = await context.Stations.ToListAsync();

        if (!users.Any() || !vehicles.Any() || !stations.Any())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần có Users, Vehicles và Stations trước khi seed Bookings."));
        }

        // Find specific users or use first available renters
        var userA = users.FirstOrDefault(u => u.Email == "nguyenvana@gmail.com") ?? users[0];
        var userB = users.Count > 1 ? (users.FirstOrDefault(u => u.Email == "tranthib@gmail.com") ?? users[1]) : users[0];

        // Get stations
        var station1 = stations.FirstOrDefault() ?? stations[0];
        var station2 = stations.Count > 1 ? stations[1] : stations[0];
        var station3 = stations.Count > 2 ? stations[2] : stations[0];

        // Seed Bookings - Nhiều Pending bookings
        var bookings = new[]
        {
            // Pending Bookings (Nhiều hơn để test)
            new Booking
            {
                BookingCode = "BK001",
                UserId = userA.Id,
                VehicleId = vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddDays(-2),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(1),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(2),
                Status = BookingStatus.Pending,
                Notes = "Cần xe gấp cho công việc",
                CreatedAt = DateTime.UtcNow.AddDays(-2)
            },
            new Booking
            {
                BookingCode = "BK002",
                UserId = userA.Id,
                VehicleId = vehicles.Count > 1 ? vehicles[1].Id : vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddHours(-12),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(6),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(1),
                Status = BookingStatus.Pending,
                Notes = "Đặt xe đi họp khách hàng",
                CreatedAt = DateTime.UtcNow.AddHours(-12)
            },
            new Booking
            {
                BookingCode = "BK003",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddHours(-8),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(10),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(2),
                Status = BookingStatus.Pending,
                Notes = "Thuê xe đi chơi cuối tuần",
                CreatedAt = DateTime.UtcNow.AddHours(-8)
            },
            new Booking
            {
                BookingCode = "BK004",
                UserId = userA.Id,
                VehicleId = vehicles.Count > 3 ? vehicles[3].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddHours(-3),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(3),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(5),
                Status = BookingStatus.Pending,
                Notes = "Đặt trước cho tuần sau",
                CreatedAt = DateTime.UtcNow.AddHours(-3)
            },
            new Booking
            {
                BookingCode = "BK005",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 4 ? vehicles[4].Id : vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddHours(-1),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(4),
                ScheduledReturnTime = DateTime.UtcNow.AddHours(8),
                Status = BookingStatus.Pending,
                Notes = "Thuê xe vài giờ đi giao hàng",
                CreatedAt = DateTime.UtcNow.AddHours(-1)
            },
            // Confirmed Bookings
            new Booking
            {
                BookingCode = "BK006",
                UserId = userA.Id,
                VehicleId = vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddDays(-5),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(2),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(1),
                Status = BookingStatus.Confirmed,
                Notes = "Đã xác nhận, sẵn sàng nhận xe",
                CreatedAt = DateTime.UtcNow.AddDays(-5)
            },
            new Booking
            {
                BookingCode = "BK007",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 2 ? vehicles[2].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddDays(-3),
                ScheduledPickupTime = DateTime.UtcNow.AddHours(5),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(3),
                Status = BookingStatus.Confirmed,
                Notes = "Thuê xe đi du lịch 3 ngày",
                CreatedAt = DateTime.UtcNow.AddDays(-3)
            },
            // Completed Bookings
            new Booking
            {
                BookingCode = "BK008",
                UserId = userB.Id,
                VehicleId = vehicles.Count > 3 ? vehicles[3].Id : vehicles[0].Id,
                StationId = station2.Id,
                BookingDate = DateTime.UtcNow.AddDays(-10),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(-8),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(-7),
                Status = BookingStatus.Completed,
                Notes = "Hoàn thành tốt",
                CreatedAt = DateTime.UtcNow.AddDays(-10),
                UpdatedAt = DateTime.UtcNow.AddDays(-7)
            },
            new Booking
            {
                BookingCode = "BK009",
                UserId = userA.Id,
                VehicleId = vehicles[0].Id,
                StationId = station1.Id,
                BookingDate = DateTime.UtcNow.AddDays(-15),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(-14),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(-13),
                Status = BookingStatus.Completed,
                Notes = "Khách hàng thân thiết",
                CreatedAt = DateTime.UtcNow.AddDays(-15),
                UpdatedAt = DateTime.UtcNow.AddDays(-13)
            },
            // Cancelled Bookings
            new Booking
            {
                BookingCode = "BK010",
                UserId = userA.Id,
                VehicleId = vehicles.Count > 4 ? vehicles[4].Id : vehicles[0].Id,
                StationId = station3.Id,
                BookingDate = DateTime.UtcNow.AddDays(-4),
                ScheduledPickupTime = DateTime.UtcNow.AddDays(-3),
                ScheduledReturnTime = DateTime.UtcNow.AddDays(-2),
                Status = BookingStatus.Cancelled,
                Notes = "Khách hủy vì thay đổi kế hoạch",
                CreatedAt = DateTime.UtcNow.AddDays(-4),
                UpdatedAt = DateTime.UtcNow.AddDays(-3)
            }
        };

        context.Bookings.AddRange(bookings);
        
        // Update vehicle status for confirmed/pending bookings
        foreach (var booking in bookings.Where(b => b.Status == BookingStatus.Pending || b.Status == BookingStatus.Confirmed))
        {
            var vehicle = vehicles.FirstOrDefault(v => v.Id == booking.VehicleId);
            if (vehicle != null)
            {
                vehicle.Status = VehicleStatus.Booked;
                vehicle.UpdatedAt = DateTime.UtcNow;
            }
        }
        
        await context.SaveChangesAsync();

        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Seed bookings thành công!",
            count = bookings.Length
        }));
    }

    /// <summary>
    /// Seed vehicles data (Admin only - Development)
    /// </summary>
    [HttpPost("seed/vehicles")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    public async Task<IActionResult> SeedVehicles([FromServices] ApplicationDbContext context)
    {
        // Get existing stations
        var stations = await context.Stations.ToListAsync();

        if (!stations.Any())
        {
            return BadRequest(ApiResponse<object>.ErrorResponse("Cần có Stations trước khi seed Vehicles."));
        }

        var station1 = stations.FirstOrDefault() ?? stations[0];
        var station2 = stations.Count > 1 ? stations[1] : stations[0];
        var station3 = stations.Count > 2 ? stations[2] : stations[0];

        // Seed more vehicles
        var vehicles = new[]
        {
            // Station 1 - Quận 1 (10 xe với đầy đủ status)
            
            // Available (2 xe)
            new Vehicle
            {
                LicensePlate = "59A-11101",
                Model = "Feliz S",
                Brand = "VinFast",
                Year = 2024,
                Color = "Trắng",
                BatteryCapacity = 100,
                PricePerHour = 55000,
                PricePerDay = 320000,
                Status = VehicleStatus.Available,
                StationId = station1.Id,
                Description = "Xe máy điện VinFast Feliz S 2024, pin mới",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59A-11102",
                Model = "Evo200",
                Brand = "Pega",
                Year = 2023,
                Color = "Đen",
                BatteryCapacity = 95,
                PricePerHour = 48000,
                PricePerDay = 290000,
                Status = VehicleStatus.Available,
                StationId = station1.Id,
                Description = "Xe máy điện Pega Evo200",
                CreatedAt = DateTime.UtcNow
            },
            
            // Booked (2 xe)
            new Vehicle
            {
                LicensePlate = "59A-11103",
                Model = "Klara S",
                Brand = "VinFast",
                Year = 2023,
                Color = "Xanh dương",
                BatteryCapacity = 88,
                PricePerHour = 50000,
                PricePerDay = 300000,
                Status = VehicleStatus.Booked,
                StationId = station1.Id,
                Description = "Xe đã được đặt trước",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59A-11104",
                Model = "Ludo",
                Brand = "VinFast",
                Year = 2024,
                Color = "Hồng",
                BatteryCapacity = 100,
                PricePerHour = 52000,
                PricePerDay = 310000,
                Status = VehicleStatus.Booked,
                StationId = station1.Id,
                Description = "Xe đã được đặt trước, chờ khách lấy",
                CreatedAt = DateTime.UtcNow
            },
            
            // InUse (2 xe)
            new Vehicle
            {
                LicensePlate = "59A-11105",
                Model = "Impes",
                Brand = "VinFast",
                Year = 2023,
                Color = "Đỏ",
                BatteryCapacity = 85,
                PricePerHour = 49000,
                PricePerDay = 295000,
                Status = VehicleStatus.InUse,
                StationId = station1.Id,
                Description = "Xe đang được thuê, khách đang sử dụng",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59A-11106",
                Model = "T9",
                Brand = "Yadea",
                Year = 2024,
                Color = "Xanh lá",
                BatteryCapacity = 78,
                PricePerHour = 47000,
                PricePerDay = 285000,
                Status = VehicleStatus.InUse,
                StationId = station1.Id,
                Description = "Xe đang được sử dụng",
                CreatedAt = DateTime.UtcNow
            },
            
            // Maintenance (2 xe)
            new Vehicle
            {
                LicensePlate = "59A-11107",
                Model = "Klara",
                Brand = "VinFast",
                Year = 2022,
                Color = "Xám",
                BatteryCapacity = 70,
                PricePerHour = 45000,
                PricePerDay = 280000,
                Status = VehicleStatus.Maintenance,
                StationId = station1.Id,
                Description = "Xe đang bảo trì định kỳ",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59A-11108",
                Model = "Xmen",
                Brand = "Yadea",
                Year = 2023,
                Color = "Vàng",
                BatteryCapacity = 60,
                PricePerHour = 42000,
                PricePerDay = 260000,
                Status = VehicleStatus.Maintenance,
                StationId = station1.Id,
                Description = "Xe đang thay thế pin",
                CreatedAt = DateTime.UtcNow
            },
            
            // Damaged (2 xe)
            new Vehicle
            {
                LicensePlate = "59A-11109",
                Model = "Bizin",
                Brand = "Yadea",
                Year = 2023,
                Color = "Cam",
                BatteryCapacity = 50,
                PricePerHour = 44000,
                PricePerDay = 270000,
                Status = VehicleStatus.Damaged,
                StationId = station1.Id,
                Description = "Xe bị hư hỏng sau khi trả, cần sửa chữa",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59A-11110",
                Model = "Elite",
                Brand = "Pega",
                Year = 2022,
                Color = "Tím",
                BatteryCapacity = 40,
                PricePerHour = 46000,
                PricePerDay = 280000,
                Status = VehicleStatus.Damaged,
                StationId = station1.Id,
                Description = "Xe bị va chạm nhẹ, đang chờ sửa chữa",
                CreatedAt = DateTime.UtcNow
            },
            // Station 2 - Quận 3 (6 xe)
            
            // Available
            new Vehicle
            {
                LicensePlate = "59B-22201",
                Model = "G5",
                Brand = "Yadea",
                Year = 2024,
                Color = "Trắng",
                BatteryCapacity = 98,
                PricePerHour = 47000,
                PricePerDay = 285000,
                Status = VehicleStatus.Available,
                StationId = station2.Id,
                Description = "Xe máy điện Yadea G5 2024",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59B-22202",
                Model = "Xmen",
                Brand = "Yadea",
                Year = 2023,
                Color = "Đen",
                BatteryCapacity = 85,
                PricePerHour = 42000,
                PricePerDay = 260000,
                Status = VehicleStatus.Available,
                StationId = station2.Id,
                Description = "Xe máy điện Yadea Xmen",
                CreatedAt = DateTime.UtcNow
            },
            // Booked
            new Vehicle
            {
                LicensePlate = "59B-22203",
                Model = "Elite",
                Brand = "Pega",
                Year = 2024,
                Color = "Vàng",
                BatteryCapacity = 95,
                PricePerHour = 46000,
                PricePerDay = 280000,
                Status = VehicleStatus.Booked,
                StationId = station2.Id,
                Description = "Xe đã được đặt trước",
                CreatedAt = DateTime.UtcNow
            },
            // InUse
            new Vehicle
            {
                LicensePlate = "59B-22204",
                Model = "T9",
                Brand = "Yadea",
                Year = 2024,
                Color = "Xanh lá",
                BatteryCapacity = 80,
                PricePerHour = 47000,
                PricePerDay = 285000,
                Status = VehicleStatus.InUse,
                StationId = station2.Id,
                Description = "Xe đang được thuê",
                CreatedAt = DateTime.UtcNow
            },
            // Maintenance
            new Vehicle
            {
                LicensePlate = "59B-22205",
                Model = "Feliz",
                Brand = "VinFast",
                Year = 2023,
                Color = "Xám",
                BatteryCapacity = 70,
                PricePerHour = 52000,
                PricePerDay = 310000,
                Status = VehicleStatus.Maintenance,
                StationId = station2.Id,
                Description = "Xe đang bảo trì",
                CreatedAt = DateTime.UtcNow
            },
            // Damaged
            new Vehicle
            {
                LicensePlate = "59B-22206",
                Model = "Klara",
                Brand = "VinFast",
                Year = 2022,
                Color = "Nâu",
                BatteryCapacity = 55,
                PricePerHour = 48000,
                PricePerDay = 290000,
                Status = VehicleStatus.Damaged,
                StationId = station2.Id,
                Description = "Xe bị hư hỏng nhẹ",
                CreatedAt = DateTime.UtcNow
            },
            
            // Station 3 - Bình Thạnh (5 xe)
            
            // Available
            new Vehicle
            {
                LicensePlate = "59C-33301",
                Model = "Ludo",
                Brand = "VinFast",
                Year = 2024,
                Color = "Hồng",
                BatteryCapacity = 100,
                PricePerHour = 52000,
                PricePerDay = 310000,
                Status = VehicleStatus.Available,
                StationId = station3.Id,
                Description = "Xe máy điện VinFast Ludo 2024",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59C-33302",
                Model = "Impes",
                Brand = "VinFast",
                Year = 2023,
                Color = "Đen",
                BatteryCapacity = 90,
                PricePerHour = 49000,
                PricePerDay = 295000,
                Status = VehicleStatus.Available,
                StationId = station3.Id,
                Description = "Xe máy điện VinFast Impes",
                CreatedAt = DateTime.UtcNow
            },
            // Booked
            new Vehicle
            {
                LicensePlate = "59C-33303",
                Model = "Plus",
                Brand = "Pega",
                Year = 2024,
                Color = "Xanh",
                BatteryCapacity = 88,
                PricePerHour = 40000,
                PricePerDay = 250000,
                Status = VehicleStatus.Booked,
                StationId = station3.Id,
                Description = "Xe đã được đặt",
                CreatedAt = DateTime.UtcNow
            },
            // InUse
            new Vehicle
            {
                LicensePlate = "59C-33304",
                Model = "Bizin",
                Brand = "Yadea",
                Year = 2023,
                Color = "Trắng",
                BatteryCapacity = 75,
                PricePerHour = 44000,
                PricePerDay = 270000,
                Status = VehicleStatus.InUse,
                StationId = station3.Id,
                Description = "Xe đang được sử dụng",
                CreatedAt = DateTime.UtcNow
            },
            // Maintenance
            new Vehicle
            {
                LicensePlate = "59C-33305",
                Model = "Elite",
                Brand = "Pega",
                Year = 2022,
                Color = "Xám",
                BatteryCapacity = 65,
                PricePerHour = 43000,
                PricePerDay = 265000,
                Status = VehicleStatus.Maintenance,
                StationId = station3.Id,
                Description = "Xe đang bảo trì",
                CreatedAt = DateTime.UtcNow
            }
        };

        context.Vehicles.AddRange(vehicles);
        await context.SaveChangesAsync();

        // Tạo bookings và rentals cho vehicles có status Booked/InUse
        var users = await context.Users.Where(u => u.Role == UserRole.Renter).ToListAsync();
        if (users.Any())
        {
            var userA = users.FirstOrDefault();
            var userB = users.Count > 1 ? users[1] : users[0];
            
            // Tạo bookings cho xe có status Booked
            var bookedVehicles = vehicles.Where(v => v.Status == VehicleStatus.Booked).ToList();
            var bookings = new List<Booking>();
            
            for (int i = 0; i < bookedVehicles.Count; i++)
            {
                var vehicle = bookedVehicles[i];
                var booking = new Booking
                {
                    BookingCode = $"BK{DateTime.UtcNow.Ticks}{i}",
                    UserId = i % 2 == 0 ? userA.Id : userB.Id,
                    VehicleId = vehicle.Id,
                    StationId = vehicle.StationId,
                    BookingDate = DateTime.UtcNow.AddDays(-1),
                    ScheduledPickupTime = DateTime.UtcNow.AddHours(2 + i),
                    ScheduledReturnTime = DateTime.UtcNow.AddDays(1 + i),
                    Status = BookingStatus.Confirmed,
                    Notes = $"Booking tự động cho xe {vehicle.LicensePlate}",
                    CreatedAt = DateTime.UtcNow.AddDays(-1)
                };
                bookings.Add(booking);
            }
            
            if (bookings.Any())
            {
                context.Bookings.AddRange(bookings);
                await context.SaveChangesAsync();
            }
            
            // Tạo rentals cho xe có status InUse
            var inUseVehicles = vehicles.Where(v => v.Status == VehicleStatus.InUse).ToList();
            var rentals = new List<Rental>();
            
            for (int i = 0; i < inUseVehicles.Count; i++)
            {
                var vehicle = inUseVehicles[i];
                
                // Tạo booking trước
                var rentalBooking = new Booking
                {
                    BookingCode = $"BK-R{DateTime.UtcNow.Ticks}{i}",
                    UserId = i % 2 == 0 ? userA.Id : userB.Id,
                    VehicleId = vehicle.Id,
                    StationId = vehicle.StationId,
                    BookingDate = DateTime.UtcNow.AddDays(-2),
                    ScheduledPickupTime = DateTime.UtcNow.AddDays(-1),
                    ScheduledReturnTime = DateTime.UtcNow.AddDays(2),
                    Status = BookingStatus.Confirmed,
                    Notes = $"Booking cho rental {vehicle.LicensePlate}",
                    CreatedAt = DateTime.UtcNow.AddDays(-2)
                };
                context.Bookings.Add(rentalBooking);
                await context.SaveChangesAsync(); // Save để có BookingId
                
                // Tạo rental
                var rental = new Rental
                {
                    RentalCode = $"RN{DateTime.UtcNow.Ticks}{i}",
                    BookingId = rentalBooking.Id,
                    UserId = rentalBooking.UserId,
                    VehicleId = vehicle.Id,
                    StationId = vehicle.StationId,
                    PickupTime = DateTime.UtcNow.AddHours(-2 - i),
                    ExpectedReturnTime = DateTime.UtcNow.AddDays(2),
                    TotalAmount = vehicle.PricePerDay * 2,
                    Status = RentalStatus.Active,
                    Notes = $"Rental đang hoạt động cho xe {vehicle.LicensePlate}",
                    CreatedAt = DateTime.UtcNow.AddHours(-2 - i)
                };
                rentals.Add(rental);
            }
            
            if (rentals.Any())
            {
                context.Rentals.AddRange(rentals);
                await context.SaveChangesAsync();
            }
        }

        // Đếm bookings và rentals đã tạo
        var bookedCount = vehicles.Count(v => v.Status == VehicleStatus.Booked);
        var inUseCount = vehicles.Count(v => v.Status == VehicleStatus.InUse);
        
        return Ok(ApiResponse<object>.SuccessResponse(new
        {
            message = "Seed vehicles thành công! (Đã tự động tạo bookings/rentals cho xe Booked/InUse)",
            count = vehicles.Length,
            autoCreated = new
            {
                bookingsForBooked = bookedCount,
                bookingsAndRentalsForInUse = inUseCount
            },
            summary = new
            {
                totalVehicles = vehicles.Length,
                byStation = new
                {
                    station1 = vehicles.Count(v => v.StationId == station1.Id),
                    station2 = vehicles.Count(v => v.StationId == station2.Id),
                    station3 = vehicles.Count(v => v.StationId == station3.Id)
                },
                byStatus = new
                {
                    available = vehicles.Count(v => v.Status == VehicleStatus.Available),
                    booked = vehicles.Count(v => v.Status == VehicleStatus.Booked),
                    inUse = vehicles.Count(v => v.Status == VehicleStatus.InUse),
                    maintenance = vehicles.Count(v => v.Status == VehicleStatus.Maintenance),
                    damaged = vehicles.Count(v => v.Status == VehicleStatus.Damaged)
                }
            },
            stationBreakdown = new
            {
                station1 = new
                {
                    name = "Station 1 - Quận 1 (Main Station)",
                    total = vehicles.Count(v => v.StationId == station1.Id),
                    available = vehicles.Count(v => v.StationId == station1.Id && v.Status == VehicleStatus.Available),
                    booked = vehicles.Count(v => v.StationId == station1.Id && v.Status == VehicleStatus.Booked),
                    inUse = vehicles.Count(v => v.StationId == station1.Id && v.Status == VehicleStatus.InUse),
                    maintenance = vehicles.Count(v => v.StationId == station1.Id && v.Status == VehicleStatus.Maintenance),
                    damaged = vehicles.Count(v => v.StationId == station1.Id && v.Status == VehicleStatus.Damaged)
                },
                station2 = new
                {
                    name = "Station 2 - Quận 3",
                    total = vehicles.Count(v => v.StationId == station2.Id),
                    available = vehicles.Count(v => v.StationId == station2.Id && v.Status == VehicleStatus.Available),
                    booked = vehicles.Count(v => v.StationId == station2.Id && v.Status == VehicleStatus.Booked),
                    inUse = vehicles.Count(v => v.StationId == station2.Id && v.Status == VehicleStatus.InUse),
                    maintenance = vehicles.Count(v => v.StationId == station2.Id && v.Status == VehicleStatus.Maintenance),
                    damaged = vehicles.Count(v => v.StationId == station2.Id && v.Status == VehicleStatus.Damaged)
                },
                station3 = new
                {
                    name = "Station 3 - Bình Thạnh",
                    total = vehicles.Count(v => v.StationId == station3.Id),
                    available = vehicles.Count(v => v.StationId == station3.Id && v.Status == VehicleStatus.Available),
                    booked = vehicles.Count(v => v.StationId == station3.Id && v.Status == VehicleStatus.Booked),
                    inUse = vehicles.Count(v => v.StationId == station3.Id && v.Status == VehicleStatus.InUse),
                    maintenance = vehicles.Count(v => v.StationId == station3.Id && v.Status == VehicleStatus.Maintenance),
                    damaged = vehicles.Count(v => v.StationId == station3.Id && v.Status == VehicleStatus.Damaged)
                }
            }
        }));
    }
}

