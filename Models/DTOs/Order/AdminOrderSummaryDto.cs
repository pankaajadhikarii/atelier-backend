using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Order;

public class AdminOrderSummaryDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public string CustomerName { get; set; } = string.Empty;

    public string CustomerEmail { get; set; } = string.Empty;

    public string? CustomerPhone { get; set; }

    public string GarmentName { get; set; } = string.Empty;

    public string DesignName { get; set; } = string.Empty;

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public OrderPaymentStatus PaymentStatus { get; set; }

    public PaymentMethod PaymentMethod { get; set; }

    public DateTime OrderDate { get; set; }

    public DateTime UpdatedAt { get; set; }
}
