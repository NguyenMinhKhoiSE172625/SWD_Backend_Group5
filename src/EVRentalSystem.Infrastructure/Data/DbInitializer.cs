using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Infrastructure.Data;

public static class DbInitializer
{
    public static void Initialize(ApplicationDbContext context)
    {
        // Note: Database should already be created and migrated before calling this method
        // Do NOT call EnsureCreated() here as we're using Migrate() in Program.cs
        
        // Check if data already exists
        if (context.Users.Any())
        {
            return;
        }

        // Seed Stations
        var stations = new[]
        {
            new Station
            {
                Name = "Điểm thuê Quận 1",
                Address = "123 Nguyễn Huệ, Quận 1, TP.HCM",
                Latitude = 10.7769m,
                Longitude = 106.7009m,
                PhoneNumber = "0281234567",
                Description = "Điểm thuê xe trung tâm Quận 1",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Station
            {
                Name = "Điểm thuê Quận 3",
                Address = "456 Võ Văn Tần, Quận 3, TP.HCM",
                Latitude = 10.7829m,
                Longitude = 106.6920m,
                PhoneNumber = "0281234568",
                Description = "Điểm thuê xe Quận 3",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            },
            new Station
            {
                Name = "Điểm thuê Bình Thạnh",
                Address = "789 Điện Biên Phủ, Bình Thạnh, TP.HCM",
                Latitude = 10.8014m,
                Longitude = 106.7147m,
                PhoneNumber = "0281234569",
                Description = "Điểm thuê xe Bình Thạnh",
                IsActive = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Stations.AddRange(stations);
        context.SaveChanges();

        // Seed Users
        var users = new[]
        {
            new User
            {
                FullName = "Admin User",
                Email = "admin@evrentalsystem.com",
                PhoneNumber = "0901234567",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Admin@123"),
                Role = UserRole.Admin,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Nhân viên Quận 1",
                Email = "staff1@evrentalsystem.com",
                PhoneNumber = "0901234568",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role = UserRole.StationStaff,
                StationId = stations[0].Id,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Nhân viên Quận 3",
                Email = "staff2@evrentalsystem.com",
                PhoneNumber = "0901234569",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("Staff@123"),
                Role = UserRole.StationStaff,
                StationId = stations[1].Id,
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Nguyễn Văn A",
                Email = "nguyenvana@gmail.com",
                PhoneNumber = "0901234570",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = UserRole.Renter,
                DriverLicenseNumber = "B123456789",
                IdCardNumber = "079123456789",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            },
            new User
            {
                FullName = "Trần Thị B",
                Email = "tranthib@gmail.com",
                PhoneNumber = "0901234571",
                PasswordHash = BCrypt.Net.BCrypt.HashPassword("User@123"),
                Role = UserRole.Renter,
                DriverLicenseNumber = "B987654321",
                IdCardNumber = "079987654321",
                IsVerified = true,
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Users.AddRange(users);
        context.SaveChanges();

        // Seed Vehicles
        var vehicles = new[]
        {
            new Vehicle
            {
                LicensePlate = "59A-12345",
                Model = "VinFast Klara",
                Brand = "VinFast",
                Year = 2023,
                Color = "Đỏ",
                BatteryCapacity = 100,
                PricePerHour = 50000,
                PricePerDay = 300000,
                Status = VehicleStatus.Available,
                StationId = stations[0].Id,
                Description = "Xe máy điện VinFast Klara mới 2023",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59A-12346",
                Model = "VinFast Klara",
                Brand = "VinFast",
                Year = 2023,
                Color = "Xanh",
                BatteryCapacity = 95,
                PricePerHour = 50000,
                PricePerDay = 300000,
                Status = VehicleStatus.Available,
                StationId = stations[0].Id,
                Description = "Xe máy điện VinFast Klara mới 2023",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59B-54321",
                Model = "Yadea G5",
                Brand = "Yadea",
                Year = 2023,
                Color = "Trắng",
                BatteryCapacity = 100,
                PricePerHour = 45000,
                PricePerDay = 280000,
                Status = VehicleStatus.Available,
                StationId = stations[1].Id,
                Description = "Xe máy điện Yadea G5",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59B-54322",
                Model = "Yadea G5",
                Brand = "Yadea",
                Year = 2023,
                Color = "Đen",
                BatteryCapacity = 90,
                PricePerHour = 45000,
                PricePerDay = 280000,
                Status = VehicleStatus.Available,
                StationId = stations[1].Id,
                Description = "Xe máy điện Yadea G5",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59C-11111",
                Model = "Pega Plus",
                Brand = "Pega",
                Year = 2022,
                Color = "Xám",
                BatteryCapacity = 85,
                PricePerHour = 40000,
                PricePerDay = 250000,
                Status = VehicleStatus.Available,
                StationId = stations[2].Id,
                Description = "Xe máy điện Pega Plus",
                CreatedAt = DateTime.UtcNow
            },
            new Vehicle
            {
                LicensePlate = "59C-11112",
                Model = "Pega Plus",
                Brand = "Pega",
                Year = 2022,
                Color = "Vàng",
                BatteryCapacity = 80,
                PricePerHour = 40000,
                PricePerDay = 250000,
                Status = VehicleStatus.Maintenance,
                StationId = stations[2].Id,
                Description = "Xe máy điện Pega Plus - Đang bảo trì",
                CreatedAt = DateTime.UtcNow
            }
        };
        context.Vehicles.AddRange(vehicles);
        context.SaveChanges();
    }
}

