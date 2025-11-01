using EVRentalSystem.Application.DTOs.Payment;

namespace EVRentalSystem.Application.Interfaces;

public interface IPaymentService
{
    Task<PaymentResponse?> CreatePaymentAsync(int userId, int staffId, CreatePaymentRequest request);
    Task<List<PaymentResponse>> GetUserPaymentsAsync(int userId);
    Task<List<PaymentResponse>> GetRentalPaymentsAsync(int rentalId);
}

