using EVRentalSystem.Application.DTOs.Auth;

namespace EVRentalSystem.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> VerifyUserAsync(int userId, int staffId);
    Task<bool> ForgotPasswordAsync(ForgotPasswordRequest request);
    Task<bool> ResetPasswordAsync(ResetPasswordRequest request);
    Task<UserProfileResponse?> GetProfileAsync(int userId);
    Task<UserProfileResponse?> UpdateProfileAsync(int userId, UpdateProfileRequest request);
}

