using System.ComponentModel.DataAnnotations;
using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Order;

public class CreateOrderDto
{
    [Required]
    public int FabricId { get; set; }

    [Required]
    public int GarmentId { get; set; }

    [Required]
    public int DesignId { get; set; }

    [Required]
    public int MeasurementProfileId { get; set; }

    public FitType FitType { get; set; } = FitType.RegularFit;

    public string? CustomizationDetailsJson { get; set; }

    public string? SpecialInstructions { get; set; }

    [Range(
        0.1,
        100.0,
        ErrorMessage = "Fabric quantity must be greater than 0.")]
    public decimal FabricQuantity { get; set; } = 1.0m;

    [Required]
    public PaymentMethod PaymentMethod { get; set; } =
        PaymentMethod.CashOnDelivery;

    [Required(ErrorMessage = "Shipping address is required.")]
    [StringLength(300)]
    public string ShippingAddress { get; set; } = string.Empty;
}