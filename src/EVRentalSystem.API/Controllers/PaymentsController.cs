using EVRentalSystem.API.Filters;
using EVRentalSystem.Application.DTOs.Common;
using EVRentalSystem.Application.DTOs.Payment;
using EVRentalSystem.Application.Interfaces;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;

namespace EVRentalSystem.API.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize]
[ValidateModel]
public class PaymentsController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentsController(IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    /// <summary>
    /// Tạo thanh toán (Nhân viên)
    /// </summary>
    [HttpPost]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), 200)]
    [ProducesResponseType(typeof(ApiResponse<PaymentResponse>), 400)]
    public async Task<IActionResult> Create([FromBody] CreatePaymentRequest request)
    {
        var staffId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");

        // UserId will be fetched from Rental in PaymentService
        var userId = 0;

        var result = await _paymentService.CreatePaymentAsync(userId, staffId, request);

        if (result == null)
        {
            return BadRequest(ApiResponse<PaymentResponse>.ErrorResponse("Không thể tạo thanh toán"));
        }

        return Ok(ApiResponse<PaymentResponse>.SuccessResponse(result, "Tạo thanh toán thành công"));
    }

    /// <summary>
    /// Lấy lịch sử thanh toán của người dùng
    /// </summary>
    [HttpGet("my-payments")]
    [Authorize(Roles = "Renter")]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentResponse>>), 200)]
    public async Task<IActionResult> GetMyPayments()
    {
        var userId = int.Parse(User.FindFirst(ClaimTypes.NameIdentifier)?.Value ?? "0");
        var payments = await _paymentService.GetUserPaymentsAsync(userId);
        return Ok(ApiResponse<List<PaymentResponse>>.SuccessResponse(payments));
    }

    /// <summary>
    /// Lấy danh sách thanh toán của giao dịch thuê xe
    /// </summary>
    [HttpGet("rental/{rentalId}")]
    [Authorize(Roles = "StationStaff,Admin")]
    [ProducesResponseType(typeof(ApiResponse<List<PaymentResponse>>), 200)]
    public async Task<IActionResult> GetRentalPayments(int rentalId)
    {
        var payments = await _paymentService.GetRentalPaymentsAsync(rentalId);
        return Ok(ApiResponse<List<PaymentResponse>>.SuccessResponse(payments));
    }
}

