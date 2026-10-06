namespace Atelier_backend.Models.Configuration;

public class EsewaSettings
{
    public string PaymentUrl { get; set; } = string.Empty;
    public string StatusCheckUrl { get; set; } = string.Empty;
    public string ProductCode { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public string BackendBaseUrl { get; set; } = string.Empty;
    public string SuccessPath { get; set; } = "/api/payment/esewa/success";
    public string FailurePath { get; set; } = "/api/payment/esewa/failure";
}