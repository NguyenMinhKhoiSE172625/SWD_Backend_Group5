using EVRentalSystem.Application.DTOs.Rental;

namespace EVRentalSystem.Application.Interfaces;

public interface IRentalService
{
    Task<RentalResponse?> CreateRentalAsync(int userId, int staffId, CreateRentalRequest request);
    Task<RentalResponse?> CompleteRentalAsync(int staffId, CompleteRentalRequest request);
    Task<RentalResponse?> GetRentalByIdAsync(int rentalId);
    Task<List<RentalResponse>> GetUserRentalsAsync(int userId);
    Task<List<RentalResponse>> GetActiveRentalsAsync(int? stationId = null);
    Task<List<object>> GetRentalInspectionsAsync(int rentalId);
    Task<List<RentalResponse>> GetStationRentalsAsync(int stationId, string? status = null);
    Task<object?> GetRentalForCheckinAsync(int rentalId);
    Task<object> GetRentalHistoryAsync(int? stationId, string? type, DateTime? fromDate, DateTime? toDate, string? search, int page, int pageSize);
    Task<object> GetRentalHistoryStatisticsAsync(int? stationId, DateTime? date);
    Task<object?> GetInspectionDetailAsync(int inspectionId);
    
    // Authorization helpers
    Task<bool> VerifyRentalAccessAsync(int rentalId, int userId, string? userRole);
    Task<bool> VerifyStationAccessAsync(int userId, int stationId);
}

