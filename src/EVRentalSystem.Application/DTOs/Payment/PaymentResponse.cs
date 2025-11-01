namespace EVRentalSystem.Application.DTOs.Payment;

public class PaymentResponse
{
    public int Id { get; set; }
    public string PaymentCode { get; set; } = string.Empty;
    public decimal Amount { get; set; }
    public string Type { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public string? PaymentMethod { get; set; }
    public DateTime PaymentDate { get; set; }
    public string? Notes { get; set; }
}

