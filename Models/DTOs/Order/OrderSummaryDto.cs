using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Order;

public class OrderSummaryDto
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string GarmentName { get; set; } = string.Empty;

    public string DesignName { get; set; } = string.Empty;

    public string? DesignImageUrl { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }

    public OrderPaymentStatus PaymentStatus { get; set; }

    public DateTime OrderDate { get; set; }
}