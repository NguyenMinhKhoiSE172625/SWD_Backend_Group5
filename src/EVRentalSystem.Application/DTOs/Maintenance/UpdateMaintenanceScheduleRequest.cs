using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Maintenance;

public class UpdateMaintenanceScheduleRequest
{
    [Required(ErrorMessage = "ID lịch bảo trì là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID lịch bảo trì phải lớn hơn 0")]
    public int ScheduleId { get; set; }

    [Required(ErrorMessage = "Trạng thái là bắt buộc")]
    [Range(1, 4, ErrorMessage = "Trạng thái không hợp lệ (1=Scheduled, 2=InProgress, 3=Completed, 4=Cancelled)")]
    public int Status { get; set; }

    [StringLength(1000, ErrorMessage = "Ghi chú không được vượt quá 1000 ký tự")]
    public string? Notes { get; set; }
}

