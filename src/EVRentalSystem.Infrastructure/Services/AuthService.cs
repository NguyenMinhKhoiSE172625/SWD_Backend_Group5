using EVRentalSystem.Application.DTOs.Auth;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class AuthService : IAuthService
{
    private readonly ApplicationDbContext _context;
    private readonly IJwtService _jwtService;

    public AuthService(ApplicationDbContext context, IJwtService jwtService)
    {
        _context = context;
        _jwtService = jwtService;
    }

    public async Task<LoginResponse?> RegisterAsync(RegisterRequest request)
    {
        // Check if email already exists
        if (await _context.Users.AnyAsync(u => u.Email == request.Email))
        {
            return null;
        }

        var user = new User
        {
            FullName = request.FullName,
            Email = request.Email,
            PhoneNumber = request.PhoneNumber,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            Role = UserRole.Renter,
            DriverLicenseNumber = request.DriverLicenseNumber,
            IdCardNumber = request.IdCardNumber,
            IsVerified = false,
            CreatedAt = DateTime.UtcNow
        };

        _context.Users.Add(user);
        await _context.SaveChangesAsync();

        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            StationId = user.StationId
        };
    }

    public async Task<LoginResponse?> LoginAsync(LoginRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            return null;
        }

        var token = _jwtService.GenerateToken(user);

        return new LoginResponse
        {
            Token = token,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            Role = user.Role.ToString(),
            StationId = user.StationId
        };
    }

    public async Task<bool> VerifyUserAsync(int userId, int staffId)
    {
        var staff = await _context.Users.FindAsync(staffId);
        if (staff == null || staff.Role != UserRole.StationStaff)
        {
            return false;
        }

        var user = await _context.Users.FindAsync(userId);
        if (user == null)
        {
            return false;
        }

        user.IsVerified = true;
        user.UpdatedAt = DateTime.UtcNow;
        
        await _context.SaveChangesAsync();
        return true;
    }
}

