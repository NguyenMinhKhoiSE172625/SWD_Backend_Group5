using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Maintenance;

public class CreateMaintenanceRecordRequest
{
    [Required(ErrorMessage = "ID xe là bắt buộc")]
    [Range(1, int.MaxValue, ErrorMessage = "ID xe phải lớn hơn 0")]
    public int VehicleId { get; set; }

    public int? MaintenanceScheduleId { get; set; }

    [Required(ErrorMessage = "Loại bảo trì là bắt buộc")]
    [Range(1, 4, ErrorMessage = "Loại bảo trì không hợp lệ (1=Routine, 2=Repair, 3=Inspection, 4=BatteryCheck)")]
    public int Type { get; set; }

    [Required(ErrorMessage = "Chi phí là bắt buộc")]
    [Range(0, 100000000, ErrorMessage = "Chi phí phải từ 0-100,000,000 VNĐ")]
    public decimal Cost { get; set; }

    [Required(ErrorMessage = "Mô tả là bắt buộc")]
    [StringLength(2000, ErrorMessage = "Mô tả không được vượt quá 2000 ký tự")]
    public string Description { get; set; } = string.Empty;
}

