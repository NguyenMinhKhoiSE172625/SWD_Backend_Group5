using EVRentalSystem.Application.DTOs.Auth;

namespace EVRentalSystem.Application.Interfaces;

public interface IAuthService
{
    Task<LoginResponse?> RegisterAsync(RegisterRequest request);
    Task<LoginResponse?> LoginAsync(LoginRequest request);
    Task<bool> VerifyUserAsync(int userId, int staffId);
}

