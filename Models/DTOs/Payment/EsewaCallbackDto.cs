using System.Text.Json.Serialization;

namespace Atelier_backend.Models.DTOs.Payment;

public class EsewaCallbackDto
{
    [JsonPropertyName("transaction_code")]
    public string? TransactionCode { get; set; }

    [JsonPropertyName("status")]
    public string? Status { get; set; }

    [JsonPropertyName("total_amount")]
    [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
    public decimal TotalAmount { get; set; }

    [JsonPropertyName("transaction_uuid")]
    public string? TransactionUuid { get; set; }

    [JsonPropertyName("product_code")]
    public string? ProductCode { get; set; }

    [JsonPropertyName("signed_field_names")]
    public string? SignedFieldNames { get; set; }

    [JsonPropertyName("signature")]
    public string? Signature { get; set; }
}