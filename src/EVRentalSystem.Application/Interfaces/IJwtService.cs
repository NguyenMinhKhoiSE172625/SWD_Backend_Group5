using EVRentalSystem.Domain.Entities;

namespace EVRentalSystem.Application.Interfaces;

public interface IJwtService
{
    string GenerateToken(User user);
}

