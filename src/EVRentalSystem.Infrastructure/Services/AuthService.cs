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
    private readonly IEmailService _emailService;

    public AuthService(ApplicationDbContext context, IJwtService jwtService, IEmailService emailService)
    {
        _context = context;
        _jwtService = jwtService;
        _emailService = emailService;
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

        // Gửi email chào mừng (không chờ kết quả để không làm chậm response)
        _ = _emailService.SendWelcomeEmailAsync(user.Email, user.FullName);

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

    public async Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email);

        if (user == null)
        {
            // Return true để không tiết lộ email có tồn tại hay không (security best practice)
            return true;
        }

        // Generate reset token (plain text to send via email)
        var resetToken = Convert.ToBase64String(System.Security.Cryptography.RandomNumberGenerator.GetBytes(32))
            .Replace("+", "-")
            .Replace("/", "_")
            .Replace("=", "");

        // Hash the token before storing in database for security
        var hashedToken = BCrypt.Net.BCrypt.HashPassword(resetToken);
        
        user.PasswordResetToken = hashedToken;
        user.PasswordResetTokenExpiry = DateTime.UtcNow.AddHours(1); // Token valid for 1 hour
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        // Gửi email reset password
        var emailSent = await _emailService.SendPasswordResetEmailAsync(user.Email, resetToken, user.FullName);
        
        if (!emailSent)
        {
            // Fallback: Log token nếu email không gửi được (development)
            Console.WriteLine($"⚠️ Email service chưa được cấu hình hoặc gửi thất bại.");
            Console.WriteLine($"Password reset token for {user.Email}: {resetToken}");
            Console.WriteLine($"Reset link: /reset-password?token={resetToken}&email={user.Email}");
        }

        return true;
    }

    public async Task<bool> ResetPasswordAsync(ResetPasswordRequest request)
    {
        var user = await _context.Users
            .FirstOrDefaultAsync(u => u.Email == request.Email 
                && u.PasswordResetToken != null
                && u.PasswordResetTokenExpiry.HasValue
                && u.PasswordResetTokenExpiry.Value > DateTime.UtcNow);

        if (user == null)
        {
            return false;
        }

        // Verify the token using BCrypt (since it's hashed in DB)
        if (!BCrypt.Net.BCrypt.Verify(request.Token, user.PasswordResetToken))
        {
            return false;
        }

        // Update password
        user.PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.NewPassword);
        user.PasswordResetToken = null;
        user.PasswordResetTokenExpiry = null;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return true;
    }

    public async Task<UserProfileResponse?> GetProfileAsync(int userId)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            return null;
        }

        return new UserProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            DriverLicenseNumber = user.DriverLicenseNumber,
            IdCardNumber = user.IdCardNumber,
            IsVerified = user.IsVerified,
            StationId = user.StationId,
            CreatedAt = user.CreatedAt
        };
    }

    public async Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request)
    {
        var user = await _context.Users.FindAsync(userId);
        
        if (user == null)
        {
            return null;
        }

        user.FullName = request.FullName;
        user.PhoneNumber = request.PhoneNumber;
        user.DriverLicenseNumber = request.DriverLicenseNumber;
        user.IdCardNumber = request.IdCardNumber;
        user.UpdatedAt = DateTime.UtcNow;

        await _context.SaveChangesAsync();

        return new UserProfileResponse
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email,
            PhoneNumber = user.PhoneNumber,
            Role = user.Role.ToString(),
            DriverLicenseNumber = user.DriverLicenseNumber,
            IdCardNumber = user.IdCardNumber,
            IsVerified = user.IsVerified,
            StationId = user.StationId,
            CreatedAt = user.CreatedAt
        };
    }
}

