using Atelier_backend.Data;
using Atelier_backend.Models.DTOs.Payment;
using Atelier_backend.Models.Entities;
using Atelier_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _dbContext;

    public PaymentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<PaymentDto> InitiatePaymentAsync(
        InitiatePaymentDto initiateDto,
        string customerId)
    {
        ValidatePaymentMethod(
            initiateDto.PaymentMethod);

        var order = await _dbContext.Orders
            .Include(order => order.Payment)
            .Include(order => order.Customer)
            .SingleOrDefaultAsync(order =>
                order.Id == initiateDto.OrderId &&
                order.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        ValidateOrderForPayment(order);

        if (order.TotalAmount <= 0)
        {
            throw new InvalidOperationException(
                "Order total amount must be greater than zero.");
        }

        if (order.PaymentStatus ==
            OrderPaymentStatus.Paid ||
            order.Payment?.Status ==
            PaymentTransactionStatus.Success)
        {
            throw new InvalidOperationException(
                "This order has already been paid.");
        }

        var payment = order.Payment;

        if (payment != null &&
            payment.Status == PaymentTransactionStatus.Pending)
        {
            if (payment.PaymentMethod !=
                initiateDto.PaymentMethod)
            {
                throw new InvalidOperationException(
                    "A payment is already pending with a different payment method.");
            }

            return MapToDto(order, payment);
        }

        if (initiateDto.PaymentMethod !=
            PaymentMethod.CashOnDelivery)
        {
            throw new InvalidOperationException(
                "The selected electronic payment gateway is not configured.");
        }

        if (payment == null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                CustomerId = customerId,
                Order = order,
                Customer = order.Customer
            };

            _dbContext.Payments.Add(payment);
        }

        payment.PaymentMethod =
            initiateDto.PaymentMethod;

        payment.Amount =
            order.TotalAmount;

        payment.Status =
            PaymentTransactionStatus.Pending;

        payment.TransactionReference =
            null;

        payment.PaidAt =
            null;

        payment.GatewayResponseRaw =
            null;

        payment.UpdatedAt =
            DateTime.UtcNow;

        order.PaymentMethod =
            initiateDto.PaymentMethod;

        order.PaymentStatus =
            OrderPaymentStatus.Unpaid;

        order.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(order, payment);
    }

    public async Task<PaymentDto> VerifyPaymentAsync(
        VerifyPaymentDto verifyDto,
        string customerId)
    {
        ValidatePaymentMethod(
            verifyDto.PaymentMethod);

        if (string.IsNullOrWhiteSpace(
            verifyDto.TransactionReference))
        {
            throw new InvalidOperationException(
                "Transaction reference is required.");
        }

        var order = await _dbContext.Orders
            .Include(order => order.Payment)
            .Include(order => order.Customer)
            .SingleOrDefaultAsync(order =>
                order.Id == verifyDto.OrderId &&
                order.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (order.Status is OrderStatus.Cancelled or
            OrderStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Payment cannot be verified for this order status.");
        }

        if (order.TotalAmount <= 0)
        {
            throw new InvalidOperationException(
                "Order total amount must be greater than zero.");
        }

        var payment = order.Payment;

        if (payment == null)
        {
            throw new KeyNotFoundException(
                "No payment has been initiated for this order.");
        }

        if (payment.PaymentMethod !=
            verifyDto.PaymentMethod)
        {
            throw new InvalidOperationException(
                "Payment method does not match the initiated payment.");
        }

        if (payment.Status ==
            PaymentTransactionStatus.Success)
        {
            throw new InvalidOperationException(
                "This payment has already been verified successfully.");
        }

        if (payment.PaymentMethod ==
            PaymentMethod.CashOnDelivery)
        {
            throw new InvalidOperationException(
                "Cash on Delivery does not require gateway verification.");
        }

        throw new InvalidOperationException(
            "Gateway verification is unavailable until the eSewa/Khalti package and sandbox configuration are added.");
    }

    private static void ValidatePaymentMethod(
        PaymentMethod paymentMethod)
    {
        if (!Enum.IsDefined(paymentMethod))
        {
            throw new InvalidOperationException(
                "Unsupported payment method.");
        }
    }

    private static void ValidateOrderForPayment(
        Order order)
    {
        if (order.Status is OrderStatus.Cancelled or
            OrderStatus.Rejected or
            OrderStatus.Delivered)
        {
            throw new InvalidOperationException(
                "Payment cannot be initiated for this order status.");
        }
    }

    private static PaymentDto MapToDto(
        Order order,
        Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,

            OrderId = order.Id,

            OrderNumber =
                order.OrderNumber,

            CustomerId =
                payment.CustomerId,

            CustomerName =
                order.Customer.FullName,

            TransactionReference =
                payment.TransactionReference,

            PaymentMethod =
                payment.PaymentMethod,

            Amount =
                payment.Amount,

            Status =
                payment.Status,

            PaidAt =
                payment.PaidAt,

            CreatedAt =
                payment.CreatedAt
        };
    }
}