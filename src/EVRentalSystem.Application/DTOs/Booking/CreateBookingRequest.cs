using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Booking;

public class CreateBookingRequest
{
    [Required(ErrorMessage = "ID xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID xe phải lớn hơn 0")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "ID điểm thuê là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID điểm thuê phải lớn hơn 0")]
    public int StationId { get; set; }

    [Required(ErrorMessage = "Thời gian nhận xe dự kiến là bắt buộc")]
    public DateTime ScheduledPickupTime { get; set; }

    public DateTime? ScheduledReturnTime { get; set; }

    [StringLength(500, ErrorMessage = "Ghi chú không được vượt quá 500 ký tự")]
    public string? Notes { get; set; }
}

