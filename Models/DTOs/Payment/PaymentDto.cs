using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Payment;

public class PaymentDto
{
    public int Id { get; set; }

    public int OrderId { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public string? TransactionReference { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public decimal Amount { get; set; }

    public PaymentTransactionStatus Status { get; set; }

    public DateTime? PaidAt { get; set; }

    public DateTime CreatedAt { get; set; }
}