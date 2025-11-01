using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Rental;

public class CreateRentalRequest
{
    public int? BookingId { get; set; }

    [Required(ErrorMessage = "ID xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID xe phải lớn hơn 0")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Mức pin khi giao xe là bắt buộc")]
    [Range(0, 100, ErrorMessage = "Mức pin phải từ 0-100%")]
    public int PickupBatteryLevel { get; set; }

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? PickupNotes { get; set; }

    public List<string>? PickupImageUrls { get; set; }
}

