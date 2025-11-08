using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Rental;

public class CreateRentalRequest
{
    public int? BookingId { get; set; }

    /// <summary>
    /// ID người thuê xe (bắt buộc nếu không có BookingId - dành cho walk-in customers)
    /// </summary>
    public int? UserId { get; set; }

    [Required(ErrorMessage = "ID xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID xe phải lớn hơn 0")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Mức pin khi giao xe là bắt buộc")]
    [Range(0, 100, ErrorMessage = "Mức pin phải từ 0-100%")]
    public int PickupBatteryLevel { get; set; }

    [Range(0, double.MaxValue, ErrorMessage = "ODO phải lớn hơn hoặc bằng 0")]
    public decimal? OdometerBeforePickup { get; set; }

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? PickupNotes { get; set; }

    public List<string>? PickupImageUrls { get; set; }

    /// <summary>
    /// Chữ ký người thuê (base64 encoded image)
    /// </summary>
    public string? RenterSignature { get; set; }

    /// <summary>
    /// Chữ ký nhân viên (base64 encoded image)
    /// </summary>
    public string? StaffSignature { get; set; }
}

