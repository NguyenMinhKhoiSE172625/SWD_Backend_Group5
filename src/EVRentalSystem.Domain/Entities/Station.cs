namespace EVRentalSystem.Domain.Entities;

public class Station
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string Address { get; set; } = string.Empty;
    public decimal Latitude { get; set; }
    public decimal Longitude { get; set; }
    public string PhoneNumber { get; set; } = string.Empty;
    public string? Description { get; set; }
    public bool IsActive { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    
    // Navigation properties
    public ICollection<Vehicle> Vehicles { get; set; } = new List<Vehicle>();
    public ICollection<User> Staff { get; set; } = new List<User>();
    public ICollection<Booking> Bookings { get; set; } = new List<Booking>();
}

