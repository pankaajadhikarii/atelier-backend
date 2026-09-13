using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Payment;

public class InitiatePaymentDto
{
    public int OrderId { get; set; }

    public PaymentMethod PaymentMethod { get; set; }
}