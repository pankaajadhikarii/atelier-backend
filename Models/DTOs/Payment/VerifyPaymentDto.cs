using System.ComponentModel.DataAnnotations;
using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Payment;

public class VerifyPaymentDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public string TransactionReference { get; set; } = string.Empty;

    [Required]
    public PaymentMethod PaymentMethod { get; set; }
}