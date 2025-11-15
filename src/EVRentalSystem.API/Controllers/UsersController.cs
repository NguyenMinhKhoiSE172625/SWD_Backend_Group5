using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Auth;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class UsersController : ControllerBase
{
    private readonly ApplicationDbContext _context;

    public UsersController(ApplicationDbContext context)
    {
        _context = context;
    }

    /// <summary>
    /// Lấy thông tin profile của user hiện tại
    /// </summary>
    [HttpGet("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetProfile()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users
            .Include(u => u.Station)
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy người dùng"));
        }

        var profile = new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            DriverLicenseNumber = user.DriverLicenseNumber,
            DriverLicenseImageUrl = user.DriverLicenseImageUrl,
            IdCardNumber = user.IdCardNumber,
            IdCardImageUrl = user.IdCardImageUrl,
            IsVerified = user.IsVerified,
            StationId = user.StationId,
            StationName = user.Station?.Name,
            CreatedAt = user.CreatedAt
        };

        return Ok(ApiResponse<object>.SuccessResponse(profile));
    }

    /// <summary>
    /// Cập nhật thông tin profile
    /// </summary>
    [HttpPut("profile")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 400)]
    public async Task<IActionResult> UpdateProfile([FromBody] UpdateProfileRequest request)
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var user = await _context.Users.FindAsync(userId);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy người dùng"));
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.DriverLicenseNumber = request.DriverLicenseNumber;
        user.IdCardNumber = request.IdCardNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        var profile = new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            DriverLicenseNumber = user.DriverLicenseNumber,
            IdCardNumber = user.IdCardNumber,
            IsVerified = user.IsVerified
        };

        return Ok(ApiResponse<object>.SuccessResponse(profile, "Cập nhật profile thành công"));
    }

    /// <summary>
    /// Lấy thông tin user theo ID (Staff/Admin)
    /// </summary>
    [HttpGet("{id}")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<object>), 200)]
    [ProducesResponseType(typeof(ApiResponse<object>), 404)]
    public async Task<IActionResult> GetById(int id)
    {
        var user = await _context.Users
            .Include(u => u.Station)
            .FirstOrDefaultAsync(u => u.Id == id);

        if (user == null)
        {
            return NotFound(ApiResponse<object>.ErrorResponse("Không tìm thấy người dùng"));
        }

        var userInfo = new
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            DriverLicenseNumber = user.DriverLicenseNumber,
            DriverLicenseImageUrl = user.DriverLicenseImageUrl,
            IdCardNumber = user.IdCardNumber,
            IdCardImageUrl = user.IdCardImageUrl,
            IsVerified = user.IsVerified,
            StationId = user.StationId,
            StationName = user.Station?.Name,
            CreatedAt = user.CreatedAt
        };

        return Ok(ApiResponse<object>.SuccessResponse(userInfo));
    }
}

