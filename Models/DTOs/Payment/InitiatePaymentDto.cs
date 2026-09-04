using System.ComponentModel.DataAnnotations;
using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Payment;

public class InitiatePaymentDto
{
    [Required]
    public int OrderId { get; set; }

    [Required]
    public PaymentMethod PaymentMethod { get; set; }
}
