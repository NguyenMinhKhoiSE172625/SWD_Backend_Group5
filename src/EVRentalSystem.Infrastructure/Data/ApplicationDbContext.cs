using EVRentalSystem.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Data;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options)
    {
    }

    public DbSet<User> Users { get; set; }
    public DbSet<Station> Stations { get; set; }
    public DbSet<Vehicle> Vehicles { get; set; }
    public DbSet<Booking> Bookings { get; set; }
    public DbSet<Rental> Rentals { get; set; }
    public DbSet<VehicleInspection> VehicleInspections { get; set; }
    public DbSet<Payment> Payments { get; set; }
    public DbSet<MaintenanceSchedule> MaintenanceSchedules { get; set; }
    public DbSet<MaintenanceRecord> MaintenanceRecords { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
            entity.HasIndex(e => e.PhoneNumber); // Index for phone lookup
            entity.HasIndex(e => e.Role); // Index for role-based queries
            entity.HasIndex(e => e.IsVerified); // Index for verification status
            entity.HasIndex(e => e.StationId); // Index for station staff queries
            entity.Property(e => e.FullName).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Email).IsRequired().HasMaxLength(200);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
            entity.Property(e => e.PasswordHash).IsRequired();

            entity.HasOne(e => e.Station)
                .WithMany(s => s.Staff)
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // Station configuration
        modelBuilder.Entity<Station>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.IsActive); // Index for active station queries
            entity.HasIndex(e => new { e.Latitude, e.Longitude }); // Composite index for location-based queries
            entity.Property(e => e.Name).IsRequired().HasMaxLength(200);
            entity.Property(e => e.Address).IsRequired().HasMaxLength(500);
            entity.Property(e => e.Latitude).HasPrecision(10, 7);
            entity.Property(e => e.Longitude).HasPrecision(10, 7);
            entity.Property(e => e.PhoneNumber).IsRequired().HasMaxLength(20);
        });

        // Vehicle configuration
        modelBuilder.Entity<Vehicle>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.LicensePlate).IsUnique();
            entity.HasIndex(e => e.Status); // Index for status-based queries (Available, Booked, etc.)
            entity.HasIndex(e => e.StationId); // Index for station-based vehicle queries
            entity.HasIndex(e => new { e.StationId, e.Status }); // Composite index for available vehicles at station
            entity.Property(e => e.LicensePlate).IsRequired().HasMaxLength(20);
            entity.Property(e => e.Model).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Brand).IsRequired().HasMaxLength(100);
            entity.Property(e => e.Color).IsRequired().HasMaxLength(50);
            entity.Property(e => e.PricePerHour).HasPrecision(18, 2);
            entity.Property(e => e.PricePerDay).HasPrecision(18, 2);

            entity.HasOne(e => e.Station)
                .WithMany(s => s.Vehicles)
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Booking configuration
        modelBuilder.Entity<Booking>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.BookingCode).IsUnique();
            entity.HasIndex(e => e.UserId); // Index for user's bookings
            entity.HasIndex(e => e.VehicleId); // Index for vehicle's bookings
            entity.HasIndex(e => e.StationId); // Index for station's bookings
            entity.HasIndex(e => e.Status); // Index for status-based queries
            entity.HasIndex(e => e.BookingDate); // Index for date-based queries
            entity.HasIndex(e => new { e.UserId, e.Status }); // Composite index for user's active bookings
            entity.HasIndex(e => new { e.StationId, e.Status }); // Composite index for station's pending bookings
            entity.Property(e => e.BookingCode).IsRequired().HasMaxLength(50);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Bookings)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.Bookings)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Station)
                .WithMany(s => s.Bookings)
                .HasForeignKey(e => e.StationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Rental configuration
        modelBuilder.Entity<Rental>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.RentalCode).IsUnique();
            entity.HasIndex(e => e.UserId); // Index for user's rentals
            entity.HasIndex(e => e.VehicleId); // Index for vehicle's rentals
            entity.HasIndex(e => e.Status); // Index for status-based queries
            entity.HasIndex(e => e.PickupTime); // Index for date-based queries
            entity.HasIndex(e => new { e.UserId, e.Status }); // Composite index for user's active rentals
            entity.Property(e => e.RentalCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.TotalDistance).HasPrecision(18, 2);
            entity.Property(e => e.TotalAmount).HasPrecision(18, 2);
            entity.Property(e => e.AdditionalFees).HasPrecision(18, 2);

            entity.HasOne(e => e.Booking)
                .WithOne(b => b.Rental)
                .HasForeignKey<Rental>(e => e.BookingId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Rentals)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.Rentals)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.PickupStaff)
                .WithMany()
                .HasForeignKey(e => e.PickupStaffId)
                .OnDelete(DeleteBehavior.SetNull);

            entity.HasOne(e => e.ReturnStaff)
                .WithMany()
                .HasForeignKey(e => e.ReturnStaffId)
                .OnDelete(DeleteBehavior.SetNull);
        });

        // VehicleInspection configuration
        modelBuilder.Entity<VehicleInspection>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VehicleId); // Index for vehicle's inspections
            entity.HasIndex(e => e.RentalId); // Index for rental's inspections
            entity.HasIndex(e => e.IsPickup); // Index for pickup/return inspection queries
            entity.HasIndex(e => e.InspectionDate); // Index for date-based queries
            entity.HasIndex(e => e.InspectorId); // Index for inspector's inspections

            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.Inspections)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Rental)
                .WithMany(r => r.Inspections)
                .HasForeignKey(e => e.RentalId)
                .OnDelete(DeleteBehavior.Cascade);

            entity.HasOne(e => e.Inspector)
                .WithMany()
                .HasForeignKey(e => e.InspectorId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentCode).IsUnique();
            entity.HasIndex(e => e.UserId); // Index for user's payments
            entity.HasIndex(e => e.RentalId); // Index for rental's payments
            entity.HasIndex(e => e.Status); // Index for status-based queries
            entity.HasIndex(e => e.PaymentDate); // Index for date-based queries
            entity.HasIndex(e => new { e.UserId, e.Status }); // Composite index for user's payment history
            entity.Property(e => e.PaymentCode).IsRequired().HasMaxLength(50);
            entity.Property(e => e.Amount).HasPrecision(18, 2);

            entity.HasOne(e => e.User)
                .WithMany(u => u.Payments)
                .HasForeignKey(e => e.UserId)
                .OnDelete(DeleteBehavior.Restrict);

            entity.HasOne(e => e.Rental)
                .WithMany(r => r.Payments)
                .HasForeignKey(e => e.RentalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceSchedule configuration
        modelBuilder.Entity<MaintenanceSchedule>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.ScheduledDate);
            entity.HasIndex(e => e.Status);
            entity.HasIndex(e => new { e.VehicleId, e.Status }); // Composite index

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // MaintenanceRecord configuration
        modelBuilder.Entity<MaintenanceRecord>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.VehicleId);
            entity.HasIndex(e => e.MaintenanceDate);
            entity.HasIndex(e => e.TechnicianId);
            entity.Property(e => e.Cost).HasPrecision(18, 2);

            entity.HasOne(e => e.Vehicle)
                .WithMany()
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict); // Changed from Cascade to Restrict

            entity.HasOne(e => e.MaintenanceSchedule)
                .WithMany()
                .HasForeignKey(e => e.MaintenanceScheduleId)
                .OnDelete(DeleteBehavior.SetNull);
        });
    }
}

