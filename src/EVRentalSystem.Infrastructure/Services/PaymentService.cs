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

        var payment = new Payment
        {
            PaymentCode = GeneratePaymentCode(),
            UserId = userId,
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
        return $"PAY{DateTime.UtcNow:yyyyMMddHHmmss}{new Random().Next(1000, 9999)}";
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

