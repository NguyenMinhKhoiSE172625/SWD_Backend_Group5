using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Payment;

public class CreatePaymentRequest
{
    public int? RentalId { get; set; }

    [Required(ErrorMessage = "Số tiền là bắt buộc")]
    [Range(0.01, 100000000, ErrorMessage = "Số tiền phải từ 0.01-100,000,000 VNĐ")]
    public decimal Amount { get; set; }

    [Required(ErrorMessage = "Loại thanh toán là bắt buộc")]
    [Range(0, 3, ErrorMessage = "Loại thanh toán không hợp lệ (0=Deposit, 1=RentalFee, 2=AdditionalFee, 3=Refund)")]
    public int Type { get; set; } // 0=Deposit, 1=RentalFee, 2=AdditionalFee, 3=Refund

    [Required(ErrorMessage = "Phương thức thanh toán là bắt buộc")]
    [StringLength(50, ErrorMessage = "Phương thức thanh toán không được vượt quá 50 ký tự")]
    public string PaymentMethod { get; set; } = string.Empty; // Cash, Card, BankTransfer, etc.

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    public string? Notes { get; set; }
}

