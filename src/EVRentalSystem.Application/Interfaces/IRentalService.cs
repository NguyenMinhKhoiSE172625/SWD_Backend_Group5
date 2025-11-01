using EVRentalSystem.Application.DTOs.Rental;

namespace EVRentalSystem.Application.Interfaces;

public interface IRentalService
{
    Task<RentalResponse?> CreateRentalAsync(int userId, int staffId, CreateRentalRequest request);
    Task<RentalResponse?> CompleteRentalAsync(int staffId, CompleteRentalRequest request);
    Task<RentalResponse?> GetRentalByIdAsync(int rentalId);
    Task<List<RentalResponse>> GetUserRentalsAsync(int userId);
    Task<List<RentalResponse>> GetActiveRentalsAsync();
}

