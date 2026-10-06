namespace Atelier_backend.Models.Configuration;

public class FrontendSettings
{
    public string BaseUrl { get; set; } = string.Empty;
    public string PaymentSuccessPath { get; set; } = "/payment/success";
    public string PaymentFailurePath { get; set; } = "/payment/failure";
}
