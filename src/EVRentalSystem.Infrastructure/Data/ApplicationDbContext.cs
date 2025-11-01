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

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        // User configuration
        modelBuilder.Entity<User>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.Email).IsUnique();
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
            entity.Property(e => e.RentalCode).IsRequired().HasMaxLength(50);
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
        });

        // VehicleInspection configuration
        modelBuilder.Entity<VehicleInspection>(entity =>
        {
            entity.HasKey(e => e.Id);
            
            entity.HasOne(e => e.Vehicle)
                .WithMany(v => v.Inspections)
                .HasForeignKey(e => e.VehicleId)
                .OnDelete(DeleteBehavior.Restrict);
                
            entity.HasOne(e => e.Rental)
                .WithMany(r => r.Inspections)
                .HasForeignKey(e => e.RentalId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        // Payment configuration
        modelBuilder.Entity<Payment>(entity =>
        {
            entity.HasKey(e => e.Id);
            entity.HasIndex(e => e.PaymentCode).IsUnique();
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
    }
}

