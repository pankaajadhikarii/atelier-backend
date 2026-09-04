using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.Entities;

public class Payment
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public string? TransactionReference { get; set; }
    public PaymentMethod PaymentMethod { get; set; }
    public decimal Amount { get; set; }
    public PaymentTransactionStatus Status { get; set; } = PaymentTransactionStatus.Pending;
    public DateTime? PaidAt { get; set; }
    public string? GatewayResponseRaw { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
}
