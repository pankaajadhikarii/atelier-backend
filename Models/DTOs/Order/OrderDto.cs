using Atelier_backend.Models.DTOs.Payment;
using Atelier_backend.Models.DTOs.Review;
using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Order;

public class OrderDto
{
    public int Id { get; set; }
    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;
    public string CustomerEmail { get; set; } = string.Empty;
    public string? CustomerPhone { get; set; }

    public int FabricId { get; set; }
    public string FabricName { get; set; } = string.Empty;
    public string? FabricImageUrl { get; set; }

    public int GarmentId { get; set; }
    public string GarmentName { get; set; } = string.Empty;

    public int DesignId { get; set; }
    public string DesignName { get; set; } = string.Empty;
    public string? DesignImageUrl { get; set; }

    public int? MeasurementProfileId { get; set; }

    // Created by the backend when the order is placed
    public string MeasurementSnapshotJson { get; set; } = "{}";

    public FitType FitType { get; set; }

    public string? CustomizationDetailsJson { get; set; }
    public string? SpecialInstructions { get; set; }

    public decimal FabricQuantity { get; set; }
    public decimal FabricPrice { get; set; }
    public decimal TailoringPrice { get; set; }
    public decimal CustomizationPrice { get; set; }
    public decimal DeliveryFee { get; set; }
    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; }
    public string? RejectionReason { get; set; }

    public OrderPaymentStatus PaymentStatus { get; set; }
    public PaymentMethod PaymentMethod { get; set; }

    public string ShippingAddress { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; }
    public DateTime UpdatedAt { get; set; }

    // May be null if payment/review hasn't happened yet
    public PaymentDto? Payment { get; set; }
    public ReviewDto? Review { get; set; }
}