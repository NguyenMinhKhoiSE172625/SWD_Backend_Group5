using EVRentalSystem.Application.DTOs.Payment;
using EVRentalSystem.Application.Interfaces;
using EVRentalSystem.Domain.Entities;
using EVRentalSystem.Domain.Enums;
using EVRentalSystem.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace EVRentalSystem.Infrastructure.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _context;

    public PaymentService(ApplicationDbContext context)
    {
        _context = context;
    }

    public async Task<PaymentResponse?> CreatePaymentAsync(int userId, int staffId, CreatePaymentRequest request)
    {
        // Validate payment type
        if (!Enum.IsDefined(typeof(PaymentType), request.Type))
        {
            return null;
        }

        var paymentType = (PaymentType)request.Type;

        // If userId is 0 or not provided, fetch from Rental
        var actualUserId = userId;
        if (userId == 0 && request.RentalId.HasValue)
        {
            var rental = await _context.Rentals.FindAsync(request.RentalId.Value);
            if (rental == null)
            {
                return null;
            }
            actualUserId = rental.UserId;
        }

        var payment = new Payment
        {
            PaymentCode = GeneratePaymentCode(),
            UserId = actualUserId,
            RentalId = request.RentalId,
            Amount = request.Amount,
            Type = paymentType,
            Status = PaymentStatus.Completed,
            PaymentMethod = request.PaymentMethod,
            Notes = request.Notes,
            PaymentDate = DateTime.UtcNow,
            CreatedAt = DateTime.UtcNow
        };

        _context.Payments.Add(payment);
        await _context.SaveChangesAsync();

        return MapToResponse(payment);
    }

    public async Task<List<PaymentResponse>> GetUserPaymentsAsync(int userId)
    {
        var payments = await _context.Payments
            .Where(p => p.UserId == userId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(MapToResponse).ToList();
    }

    public async Task<List<PaymentResponse>> GetRentalPaymentsAsync(int rentalId)
    {
        var payments = await _context.Payments
            .Where(p => p.RentalId == rentalId)
            .OrderByDescending(p => p.PaymentDate)
            .ToListAsync();

        return payments.Select(MapToResponse).ToList();
    }

    private string GeneratePaymentCode()
    {
        // Use RandomNumberGenerator for thread-safety and better randomness
        var randomBytes = new byte[2];
        System.Security.Cryptography.RandomNumberGenerator.Fill(randomBytes);
        var randomNumber = Math.Abs(BitConverter.ToInt16(randomBytes, 0)) % 9000 + 1000;
        return $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{randomNumber}";
    }

    private PaymentResponse MapToResponse(Payment payment)
    {
        return new PaymentResponse
        {
            Id = payment.Id,
            PaymentCode = payment.PaymentCode,
            Amount = payment.Amount,
            Type = payment.Type.ToString(),
            Status = payment.Status.ToString(),
            PaymentMethod = payment.PaymentMethod,
            PaymentDate = payment.PaymentDate,
            Notes = payment.Notes
        };
    }
}

