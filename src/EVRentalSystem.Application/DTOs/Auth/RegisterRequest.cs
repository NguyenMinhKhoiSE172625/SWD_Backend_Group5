using System.ComponentModel.DataAnnotations;

namespace EVRentalSystem.Application.DTOs.Auth;

public class RegisterRequest
{
    [Required(ErrorMessage = "Họ tên là bắt buộc")]
    [StringLength(100, MinimumLength = 2, ErrorMessage = "Họ tên phải từ 2-100 ký tự")]
    public string FullName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email là bắt buộc")]
    [EmailAddress(ErrorMessage = "Email không hợp lệ")]
    [StringLength(100, ErrorMessage = "Email không được vượt quá 100 ký tự")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Số điện thoại là bắt buộc")]
    [Phone(ErrorMessage = "Số điện thoại không hợp lệ")]
    [RegularExpression(@"^(0|\+84)[0-9]{9,10}$", ErrorMessage = "Số điện thoại phải là số Việt Nam hợp lệ")]
    public string PhoneNumber { get; set; } = string.Empty;

    [Required(ErrorMessage = "Mật khẩu là bắt buộc")]
    [StringLength(100, MinimumLength = 6, ErrorMessage = "Mật khẩu phải từ 6-100 ký tự")]
    [RegularExpression(@"^(?=.*[a-z])(?=.*[A-Z])(?=.*\d)(?=.*[@$!%*?&])[A-Za-z\d@$!%*?&]{6,}$",
        ErrorMessage = "Mật khẩu phải chứa ít nhất 1 chữ hoa, 1 chữ thường, 1 số và 1 ký tự đặc biệt")]
    public string Password { get; set; } = string.Empty;

    [StringLength(20, ErrorMessage = "Số giấy phép lái xe không được vượt quá 20 ký tự")]
    public string? DriverLicenseNumber { get; set; }

    [StringLength(20, ErrorMessage = "Số CMND/CCCD không được vượt quá 20 ký tự")]
    [RegularExpression(@"^[0-9]{9,12}$", ErrorMessage = "Số CMND/CCCD phải là 9-12 chữ số")]
    public string? IdCardNumber { get; set; }
}

