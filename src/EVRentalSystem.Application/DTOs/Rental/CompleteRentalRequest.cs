using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Rental;

public class CompleteRentalRequest
{
    [Required(ErrorMessage = "ID giao dịch thuê xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID giao dịch thuê xe phải lớn hơn 0")]
    public int RentalId { get; set; }

    [Required(ErrorMessage = "Mức pin khi trả xe là bắt buộc")]
    [Range(0, 100, ErrorMessage = "Mức pin phải từ 0-100%")]
    public int ReturnBatteryLevel { get; set; }

    [Range(0, 10000, ErrorMessage = "Tổng quãng đường phải từ 0-10000 km")]
    public decimal? TotalDistance { get; set; }

    [Range(0, 100000000, ErrorMessage = "Phí phụ thu phải từ 0-100,000,000 VNĐ")]
    public decimal? AdditionalFees { get; set; }

    [StringLength(500, ErrorMessage = "Lý do phí phụ thu không được vượt quá 500 ký tự")]
    public string? AdditionalFeesReason { get; set; }

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? ReturnNotes { get; set; }

    [StringLength(2000, ErrorMessage = "Báo cáo hư hỏng không được vượt quá 2000 ký tự")]
    public string? DamageReport { get; set; }

    public List<string>? ReturnImageUrls { get; set; }
}

