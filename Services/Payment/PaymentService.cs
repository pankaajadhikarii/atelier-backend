using System.Globalization;
using System.Net.Http.Json;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using Atelier_backend.Data;
using Atelier_backend.Models.Configuration;
using Atelier_backend.Models.DTOs.Payment;
using Atelier_backend.Models.Entities;
using Atelier_backend.Models.Enums;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace Atelier_backend.Services;

public class PaymentService : IPaymentService
{
    private readonly ApplicationDbContext _dbContext;
    private readonly HttpClient _httpClient;
    private readonly EsewaSettings _esewaSettings;

    public PaymentService(
        ApplicationDbContext dbContext,
        IHttpClientFactory httpClientFactory,
        IOptions<EsewaSettings> esewaOptions)
    {
        _dbContext = dbContext;
        _httpClient = httpClientFactory.CreateClient();
        _esewaSettings = esewaOptions.Value;
    }

    public async Task<PaymentInitiationDto> InitiateEsewaPaymentAsync(
        InitiatePaymentDto initiateDto,
        string customerId)
    {
        ValidatePaymentMethod(initiateDto.PaymentMethod);

        if (initiateDto.PaymentMethod != PaymentMethod.ESewa)
        {
            throw new InvalidOperationException(
                "This endpoint only initiates eSewa payments.");
        }

        ValidateEsewaConfiguration();

        var order = await _dbContext.Orders
            .Include(item => item.Payment)
            .Include(item => item.Customer)
            .SingleOrDefaultAsync(item =>
                item.Id == initiateDto.OrderId &&
                item.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        ValidateOrderForPayment(order);

        if (order.TotalAmount <= 0)
        {
            throw new InvalidOperationException(
                "Order total amount must be greater than zero.");
        }

        if (order.PaymentStatus == OrderPaymentStatus.Paid ||
            order.Payment?.Status == PaymentTransactionStatus.Success)
        {
            throw new InvalidOperationException(
                "This order has already been paid.");
        }

        var payment = order.Payment ?? new Payment
        {
            OrderId = order.Id,
            CustomerId = customerId,
            Order = order,
            Customer = order.Customer
        };

        if (order.Payment == null)
        {
            _dbContext.Payments.Add(payment);
        }

        var transactionUuid = $"{order.Id}-{Guid.NewGuid():N}";

        payment.PaymentMethod = PaymentMethod.ESewa;
        payment.Amount = order.TotalAmount;
        payment.Status = PaymentTransactionStatus.Pending;
        payment.TransactionReference = transactionUuid;
        payment.PaidAt = null;
        payment.GatewayResponseRaw = null;
        payment.UpdatedAt = DateTime.UtcNow;

        order.PaymentMethod = PaymentMethod.ESewa;
        order.PaymentStatus = OrderPaymentStatus.PendingVerification;
        order.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        var amount = order.TotalAmount.ToString(
            "0.00", CultureInfo.InvariantCulture);
        var fields = new Dictionary<string, string>
        {
            ["amount"] = amount,
            ["tax_amount"] = "0",
            ["total_amount"] = amount,
            ["transaction_uuid"] = transactionUuid,
            ["product_code"] = _esewaSettings.ProductCode,
            ["product_service_charge"] = "0",
            ["product_delivery_charge"] = "0",
            ["success_url"] = BuildCallbackUrl(
                _esewaSettings.SuccessPath),
            ["failure_url"] = BuildCallbackUrl(
                _esewaSettings.FailurePath),
            ["signed_field_names"] =
                "total_amount,transaction_uuid,product_code",
            ["signature"] = CreateSignature(
                $"total_amount={amount},transaction_uuid={transactionUuid},product_code={_esewaSettings.ProductCode}")
        };

        return new PaymentInitiationDto
        {
            Payment = MapToDto(order, payment),
            PaymentUrl = _esewaSettings.PaymentUrl,
            FormFields = fields
        };
    }

    public async Task<PaymentDto> InitiatePaymentAsync(
        InitiatePaymentDto initiateDto,
        string customerId)
    {
        ValidatePaymentMethod(
            initiateDto.PaymentMethod);

        var order = await _dbContext.Orders
            .Include(order => order.Payment)
            .Include(order => order.Customer)
            .SingleOrDefaultAsync(order =>
                order.Id == initiateDto.OrderId &&
                order.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        ValidateOrderForPayment(order);

        if (order.TotalAmount <= 0)
        {
            throw new InvalidOperationException(
                "Order total amount must be greater than zero.");
        }

        if (order.PaymentStatus ==
            OrderPaymentStatus.Paid ||
            order.Payment?.Status ==
            PaymentTransactionStatus.Success)
        {
            throw new InvalidOperationException(
                "This order has already been paid.");
        }

        var payment = order.Payment;

        if (payment != null &&
            payment.Status == PaymentTransactionStatus.Pending)
        {
            if (payment.PaymentMethod !=
                initiateDto.PaymentMethod)
            {
                throw new InvalidOperationException(
                    "A payment is already pending with a different payment method.");
            }

            return MapToDto(order, payment);
        }

        if (initiateDto.PaymentMethod !=
            PaymentMethod.CashOnDelivery)
        {
            throw new InvalidOperationException(
                "The selected electronic payment gateway is not configured.");
        }

        if (payment == null)
        {
            payment = new Payment
            {
                OrderId = order.Id,
                CustomerId = customerId,
                Order = order,
                Customer = order.Customer
            };

            _dbContext.Payments.Add(payment);
        }

        payment.PaymentMethod =
            initiateDto.PaymentMethod;

        payment.Amount =
            order.TotalAmount;

        payment.Status =
            PaymentTransactionStatus.Pending;

        payment.TransactionReference =
            null;

        payment.PaidAt =
            null;

        payment.GatewayResponseRaw =
            null;

        payment.UpdatedAt =
            DateTime.UtcNow;

        order.PaymentMethod =
            initiateDto.PaymentMethod;

        order.PaymentStatus =
            OrderPaymentStatus.Unpaid;

        order.UpdatedAt =
            DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(order, payment);
    }

    public async Task<PaymentDto> VerifyPaymentAsync(
        VerifyPaymentDto verifyDto,
        string customerId)
    {
        ValidatePaymentMethod(
            verifyDto.PaymentMethod);

        if (string.IsNullOrWhiteSpace(
            verifyDto.TransactionReference))
        {
            throw new InvalidOperationException(
                "Transaction reference is required.");
        }

        var order = await _dbContext.Orders
            .Include(order => order.Payment)
            .Include(order => order.Customer)
            .SingleOrDefaultAsync(order =>
                order.Id == verifyDto.OrderId &&
                order.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException(
                "Order not found.");
        }

        if (order.Status is OrderStatus.Cancelled or
            OrderStatus.Rejected)
        {
            throw new InvalidOperationException(
                "Payment cannot be verified for this order status.");
        }

        if (order.TotalAmount <= 0)
        {
            throw new InvalidOperationException(
                "Order total amount must be greater than zero.");
        }

        var payment = order.Payment;

        if (payment == null)
        {
            throw new KeyNotFoundException(
                "No payment has been initiated for this order.");
        }

        if (payment.PaymentMethod !=
            verifyDto.PaymentMethod)
        {
            throw new InvalidOperationException(
                "Payment method does not match the initiated payment.");
        }

        if (payment.Status ==
            PaymentTransactionStatus.Success)
        {
            throw new InvalidOperationException(
                "This payment has already been verified successfully.");
        }

        if (payment.PaymentMethod ==
            PaymentMethod.CashOnDelivery)
        {
            throw new InvalidOperationException(
                "Cash on Delivery does not require gateway verification.");
        }

        if (payment.PaymentMethod == PaymentMethod.ESewa)
        {
            ValidateEsewaConfiguration();
            var status = await GetEsewaStatusAsync(
                payment.TransactionReference!, order.TotalAmount);

            if (!MatchesEsewaOrder(status, payment.TransactionReference!,
                order.TotalAmount) ||
                !string.Equals(status.Status, "COMPLETE",
                    StringComparison.OrdinalIgnoreCase))
            {
                throw new InvalidOperationException(
                    "eSewa has not completed this payment.");
            }

            return await MarkEsewaPaymentSuccessfulAsync(
                order,
                payment,
                status.TransactionCode ?? status.ReferenceId,
                status);
        }

        throw new InvalidOperationException("Unsupported payment gateway.");
    }

    public async Task<PaymentDto> CheckEsewaStatusAsync(
        int orderId,
        string customerId)
    {
        ValidateEsewaConfiguration();

        var order = await _dbContext.Orders
            .Include(item => item.Payment)
            .Include(item => item.Customer)
            .SingleOrDefaultAsync(item =>
                item.Id == orderId &&
                item.CustomerId == customerId);

        if (order == null)
        {
            throw new KeyNotFoundException("Order not found.");
        }

        if (order.Payment?.PaymentMethod != PaymentMethod.ESewa ||
            string.IsNullOrWhiteSpace(order.Payment.TransactionReference))
        {
            throw new InvalidOperationException(
                "No eSewa payment has been initiated for this order.");
        }

        if (order.Payment.Status == PaymentTransactionStatus.Success)
        {
            return MapToDto(order, order.Payment);
        }

        var response = await GetEsewaStatusAsync(
            order.Payment.TransactionReference,
            order.TotalAmount);

        if (!MatchesEsewaOrder(
                response,
                order.Payment.TransactionReference,
                order.TotalAmount))
        {
            throw new InvalidOperationException(
                "The eSewa status response does not match the order.");
        }

        if (string.Equals(
                response.Status,
                "COMPLETE",
                StringComparison.OrdinalIgnoreCase))
        {
            return await MarkEsewaPaymentSuccessfulAsync(
                order,
                order.Payment,
                response.TransactionCode ?? response.ReferenceId,
                response);
        }

        if (string.Equals(
                response.Status,
                "CANCELED",
                StringComparison.OrdinalIgnoreCase))
        {
            order.Payment.Status = PaymentTransactionStatus.Failed;
            order.Payment.UpdatedAt = DateTime.UtcNow;
            order.Payment.GatewayResponseRaw =
                JsonSerializer.Serialize(response);
            order.PaymentStatus = OrderPaymentStatus.Failed;
            order.UpdatedAt = DateTime.UtcNow;
            await _dbContext.SaveChangesAsync();
        }

        return MapToDto(order, order.Payment);
    }

    public async Task<PaymentDto> ProcessEsewaCallbackAsync(
        string encodedData)
    {
        ValidateEsewaConfiguration();

        EsewaCallbackDto callback;
        JsonDocument document;
        try
        {
            var normalized = encodedData.Replace('-', '+').Replace('_', '/');
            normalized = normalized.PadRight(
                normalized.Length + (4 - normalized.Length % 4) % 4, '=');
            var json = Encoding.UTF8.GetString(
                Convert.FromBase64String(normalized));
            document = JsonDocument.Parse(json);
            callback = JsonSerializer.Deserialize<EsewaCallbackDto>(json,
                new JsonSerializerOptions
                {
                    PropertyNameCaseInsensitive = true
                }) ?? throw new InvalidOperationException(
                    "Invalid eSewa callback data.");
        }
        catch (Exception ex) when (ex is FormatException or JsonException)
        {
            throw new InvalidOperationException(
                "Invalid eSewa callback data.");
        }

        if (!VerifyCallbackSignature(document, callback))
        {
            throw new InvalidOperationException(
                "Invalid eSewa callback signature.");
        }

        if (string.IsNullOrWhiteSpace(callback.TransactionUuid) ||
            !string.Equals(callback.ProductCode, _esewaSettings.ProductCode,
                StringComparison.Ordinal) ||
            !string.Equals(callback.Status, "COMPLETE",
                StringComparison.OrdinalIgnoreCase))
        {
            throw new InvalidOperationException(
                "The eSewa payment is not complete or does not match the configured product.");
        }

        var payment = await _dbContext.Payments
            .Include(item => item.Order)
            .ThenInclude(item => item.Customer)
            .SingleOrDefaultAsync(item =>
                item.TransactionReference == callback.TransactionUuid &&
                item.PaymentMethod == PaymentMethod.ESewa);

        if (payment == null ||
            payment.Amount != callback.TotalAmount ||
            payment.Order.TotalAmount != callback.TotalAmount)
        {
            throw new InvalidOperationException(
                "The eSewa payment does not match the order.");
        }

        payment.GatewayResponseRaw = document.RootElement.GetRawText();
        return await MarkEsewaPaymentSuccessfulAsync(
            payment.Order, payment, callback.TransactionCode, callback);
    }

    private async Task<EsewaStatusResponse> GetEsewaStatusAsync(
        string transactionUuid,
        decimal totalAmount)
    {
        var amount = totalAmount.ToString(
            "0.00", CultureInfo.InvariantCulture);
        var url = $"{_esewaSettings.StatusCheckUrl}?product_code=" +
            $"{Uri.EscapeDataString(_esewaSettings.ProductCode)}&" +
            $"total_amount={Uri.EscapeDataString(amount)}&" +
            $"transaction_uuid={Uri.EscapeDataString(transactionUuid)}";

        var response = await _httpClient.GetFromJsonAsync<EsewaStatusResponse>(url);
        return response ?? throw new InvalidOperationException(
            "eSewa returned an empty status response.");
    }

    private async Task<PaymentDto> MarkEsewaPaymentSuccessfulAsync(
        Order order,
        Payment payment,
        string? transactionCode,
        object gatewayResponse)
    {
        payment.Status = PaymentTransactionStatus.Success;
        payment.PaidAt = DateTime.UtcNow;
        payment.UpdatedAt = DateTime.UtcNow;
        payment.GatewayResponseRaw = JsonSerializer.Serialize(gatewayResponse);
        order.PaymentStatus = OrderPaymentStatus.Paid;
        order.UpdatedAt = DateTime.UtcNow;
        await _dbContext.SaveChangesAsync();
        return MapToDto(order, payment);
    }

    private bool MatchesEsewaOrder(
        EsewaStatusResponse response,
        string transactionUuid,
        decimal totalAmount)
    {
        return string.Equals(response.ProductCode, _esewaSettings.ProductCode,
                   StringComparison.Ordinal) &&
               string.Equals(response.TransactionUuid, transactionUuid,
                   StringComparison.Ordinal) &&
               response.TotalAmount == totalAmount;
    }

    private void ValidateEsewaConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_esewaSettings.PaymentUrl) ||
            string.IsNullOrWhiteSpace(_esewaSettings.StatusCheckUrl) ||
            string.IsNullOrWhiteSpace(_esewaSettings.ProductCode) ||
            string.IsNullOrWhiteSpace(_esewaSettings.SecretKey) ||
            string.IsNullOrWhiteSpace(_esewaSettings.BackendBaseUrl))
        {
            throw new InvalidOperationException(
                "eSewa configuration is incomplete.");
        }
    }

    private string BuildCallbackUrl(string path) =>
        $"{_esewaSettings.BackendBaseUrl.TrimEnd('/')}/{path.TrimStart('/')}";

    private string CreateSignature(string input) =>
        Convert.ToBase64String(HMACSHA256.HashData(
            Encoding.UTF8.GetBytes(_esewaSettings.SecretKey.Trim()),
            Encoding.UTF8.GetBytes(input)));

    private bool VerifyCallbackSignature(
        JsonDocument document,
        EsewaCallbackDto callback)
    {
        if (string.IsNullOrWhiteSpace(callback.SignedFieldNames) ||
            string.IsNullOrWhiteSpace(callback.Signature))
        {
            return false;
        }

        var values = callback.SignedFieldNames
            .Split(',', StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
            .Select(field => document.RootElement.TryGetProperty(field,
                out var value)
                ? $"{field}={(value.ValueKind == JsonValueKind.String
                    ? value.GetString() : value.GetRawText())}"
                : null)
            .ToList();

        if (values.Any(value => value == null))
        {
            return false;
        }

        try
        {
            var expected = Convert.FromBase64String(
                CreateSignature(string.Join(',', values!)));
            var actual = Convert.FromBase64String(callback.Signature);
            return CryptographicOperations.FixedTimeEquals(expected, actual);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private sealed class EsewaStatusResponse
    {
        [JsonPropertyName("product_code")]
        public string? ProductCode { get; set; }

        [JsonPropertyName("transaction_uuid")]
        public string? TransactionUuid { get; set; }

        [JsonPropertyName("total_amount")]
        [JsonNumberHandling(JsonNumberHandling.AllowReadingFromString)]
        public decimal TotalAmount { get; set; }

        [JsonPropertyName("status")]
        public string? Status { get; set; }

        [JsonPropertyName("transaction_code")]
        public string? TransactionCode { get; set; }

        [JsonPropertyName("ref_id")]
        public string? ReferenceId { get; set; }
    }

    private static void ValidatePaymentMethod(
        PaymentMethod paymentMethod)
    {
        if (!Enum.IsDefined(paymentMethod))
        {
            throw new InvalidOperationException(
                "Unsupported payment method.");
        }
    }

    private static void ValidateOrderForPayment(
        Order order)
    {
        if (order.Status is OrderStatus.Cancelled or
            OrderStatus.Rejected or
            OrderStatus.Delivered)
        {
            throw new InvalidOperationException(
                "Payment cannot be initiated for this order status.");
        }
    }

    private static PaymentDto MapToDto(
        Order order,
        Payment payment)
    {
        return new PaymentDto
        {
            Id = payment.Id,

            OrderId = order.Id,

            OrderNumber =
                order.OrderNumber,

            CustomerId =
                payment.CustomerId,

            CustomerName =
                order.Customer.FullName,

            TransactionReference =
                payment.TransactionReference,

            PaymentMethod =
                payment.PaymentMethod,

            Amount =
                payment.Amount,

            Status =
                payment.Status,

            PaidAt =
                payment.PaidAt,

            CreatedAt =
                payment.CreatedAt
        };
    }
}