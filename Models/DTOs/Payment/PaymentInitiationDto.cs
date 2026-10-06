namespace Atelier_backend.Models.DTOs.Payment;

public class PaymentInitiationDto
{
    public PaymentDto Payment { get; set; } = null!;
    public string? PaymentUrl { get; set; }
    public IDictionary<string, string>? FormFields { get; set; }
}