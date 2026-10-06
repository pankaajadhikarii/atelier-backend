using Atelier_backend.Models.DTOs.Payment;

namespace Atelier_backend.Services;

public interface IPaymentService
{
    Task<PaymentDto> InitiatePaymentAsync(
        InitiatePaymentDto initiateDto,
        string customerId);

    Task<PaymentInitiationDto> InitiateEsewaPaymentAsync(
        InitiatePaymentDto initiateDto,
        string customerId);

    Task<PaymentDto> VerifyPaymentAsync(
        VerifyPaymentDto verifyDto,
        string customerId);

    Task<PaymentDto> ProcessEsewaCallbackAsync(
        string encodedData);

    Task<PaymentDto> CheckEsewaStatusAsync(
        int orderId,
        string customerId);
}