using EVRentalSystem.Domain.Enums;

namespace EVRentalSystem.Domain.Entities;

public class Payment
{
    public int Id { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    
    public int UserId { get; set; }
    public User User { get; set; } = null!;
    
    public int? RentalId { get; set; }
    public Rental? Rental { get; set; }
    
    public decimal Amount { get; set; }
    public PaymentType Type { get; set; }
    public PaymentStatus Status { get; set; }
    
    public string? PaymentMethod { get; set; } // Cash, Card, etc.
    public string? TransactionId { get; set; }
    public string? Notes { get; set; }
    
    public DateTime PaymentDate { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
}

