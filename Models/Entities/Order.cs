using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.Entities;

public class Order
{
    public int Id { get; set; }

    public string OrderNumber { get; set; } = string.Empty;

    public string CustomerId { get; set; } = string.Empty;

    public int FabricId { get; set; }

    public int GarmentId { get; set; }

    public int DesignId { get; set; }

    public int? MeasurementProfileId { get; set; }

    public string MeasurementSnapshotJson { get; set; } = "{}";

    public FitType FitType { get; set; } = FitType.RegularFit;

    public string? CustomizationDetailsJson { get; set; }

    public string? SpecialInstructions { get; set; }

    public decimal FabricQuantity { get; set; }

    public decimal FabricPrice { get; set; }

    public decimal TailoringPrice { get; set; }

    public decimal CustomizationPrice { get; set; }

    public decimal DeliveryFee { get; set; }

    public decimal TotalAmount { get; set; }

    public OrderStatus Status { get; set; } = OrderStatus.Pending;

    public string? RejectionReason { get; set; }

    public OrderPaymentStatus PaymentStatus { get; set; } =
        OrderPaymentStatus.Unpaid;

    public PaymentMethod PaymentMethod { get; set; } =
        PaymentMethod.CashOnDelivery;

    public string ShippingAddress { get; set; } = string.Empty;

    public DateTime OrderDate { get; set; } = DateTime.UtcNow;

    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser Customer { get; set; } = null!;

    public Fabric Fabric { get; set; } = null!;

    public Garment Garment { get; set; } = null!;

    public Design Design { get; set; } = null!;

    public MeasurementProfile? MeasurementProfile { get; set; }

    public Payment? Payment { get; set; }

    public Review? Review { get; set; }

    public ICollection<OrderStatusHistory> StatusHistory { get; set; } =
        new List<OrderStatusHistory>();
}