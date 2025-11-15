using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Auth;

public class UpdateProfileRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải là số Việt Nam hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Số giấy phép lái xe không được vượt quá 20 ký tự")]
    public string? DriverLicenseNumber { get; set; }

    [StringLength(20, ErrorMessage = "Số CMND/CCCD không được vượt quá 20 ký tự")]
    [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "Số CMND/CCCD phải là 9-12 chữ số")]
    public string? IdCardNumber { get; set; }
}
