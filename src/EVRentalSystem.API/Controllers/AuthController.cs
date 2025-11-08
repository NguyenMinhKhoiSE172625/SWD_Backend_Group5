using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Auth;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[ValidateModel]
public class AuthController : ControllerBase
{
    private readonly IAuthService _authService;

    public AuthController(IAuthService authService)
    {
        _authService = authService;
    }

    /// <summary>
    /// Đăng ký tài khoản người thuê xe
    /// </summary>
    [HttpPost("register")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 400)]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        var result = await _authService.RegisterAsync(request);
        
        if (result == null)
        {
            return BadRequest(ApiResponse<LoginResponse>.ErrorResponse("Email đã tồn tại"));
        }

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Đăng ký thành công"));
    }

    /// <summary>
    /// Đăng nhập
    /// </summary>
    [HttpPost("login")]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<LoginResponse>), 401)]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        var result = await _authService.LoginAsync(request);
        
        if (result == null)
        {
            return Unauthorized(ApiResponse<LoginResponse>.ErrorResponse("Email hoặc mật khẩu không đúng"));
        }

        return Ok(ApiResponse<LoginResponse>.SuccessResponse(result, "Đăng nhập thành công"));
    }

    /// <summary>
    /// Xác thực khách hàng (Chỉ nhân viên)
    /// </summary>
    [HttpPost("verify/{userId}")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 404)]
    public async Task<IActionResult> VerifyUser([FromRoute] int userId)
    {
        if (userId <= 0)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("ID người dùng không hợp lệ"));
        }

        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var result = await _authService.VerifyUserAsync(userId, staffId);

        if (!result)
        {
            return NotFound(ApiResponse<bool>.ErrorResponse("Không tìm thấy người dùng hoặc người dùng đã được xác thực"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Xác thực thành công"));
    }

    /// <summary>
    /// Quên mật khẩu - Gửi email reset password
    /// </summary>
    [HttpPost("forgot-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> ForgotPassword([FromBody] ForgotPasswordRequest request)
    {
        var result = await _authService.ForgotPasswordAsync(request);
        
        // Always return success to prevent email enumeration attacks
        return Ok(ApiResponse<bool>.SuccessResponse(true, "Nếu email tồn tại, bạn sẽ nhận được link reset mật khẩu"));
    }

    /// <summary>
    /// Đặt lại mật khẩu với token
    /// </summary>
    [HttpPost("reset-password")]
    [ProducesResponseType(typeof(ApiResponse<bool>), 200)]
    [ProducesResponseType(typeof(ApiResponse<bool>), 400)]
    public async Task<IActionResult> ResetPassword([FromBody] ResetPasswordRequest request)
    {
        var result = await _authService.ResetPasswordAsync(request);
        
        if (!result)
        {
            return BadRequest(ApiResponse<bool>.ErrorResponse("Token không hợp lệ hoặc đã hết hạn"));
        }

        return Ok(ApiResponse<bool>.SuccessResponse(true, "Đặt lại mật khẩu thành công"));
    }
}

