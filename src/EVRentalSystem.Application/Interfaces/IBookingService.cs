using EVRentalSystem.Application.DTOs.Booking;

namespace EVRentalSystem.Application.Interfaces;

public interface IBookingService
{
    Task<BookingResponse?> CreateBookingAsync(int userId, CreateBookingRequest request);
    Task<BookingResponse?> GetBookingByIdAsync(int bookingId);
    Task<List<BookingResponse>> GetUserBookingsAsync(int userId);
    Task<List<BookingResponse>> GetStationBookingsAsync(int stationId);
    Task<bool> CancelBookingAsync(int bookingId, int userId);
    Task<bool> ConfirmBookingAsync(int bookingId, int staffId);
}

