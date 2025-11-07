using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Maintenance;

public class CreateMaintenanceScheduleRequest
{
    [Required(ErrorMessage = "ID xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID xe phải lớn hơn 0")]
    public int VehicleId { get; set; }

    [Required(ErrorMessage = "Ngày bảo trì là bắt buộc")]
    public DateTime ScheduledDate { get; set; }

    [Required(ErrorMessage = "Loại bảo trì là bắt buộc")]
    [Range(1, 4, ErrorMessage = "Loại bảo trì không hợp lệ (1=Routine, 2=Repair, 3=Inspection, 4=BatteryCheck)")]
    public int Type { get; set; }

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? Notes { get; set; }

    public int? AssignedTechnicianId { get; set; }
}

