using System.Security.Claims;
using Atelier_backend.Models.Enums;
using Atelier_backend.Models.Configuration;
using Atelier_backend.Models.DTOs.Payment;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;
    private readonly FrontendSettings _frontendSettings;

    public PaymentController(
        IPaymentService paymentService,
        IOptions<FrontendSettings> frontendOptions)
    {
        _paymentService = paymentService;
        _frontendSettings = frontendOptions.Value;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<PaymentInitiationDto>>
        InitiatePayment(
            InitiatePaymentDto initiateDto)
    {
        var customerId =
            GetCurrentCustomerId();

        if (customerId == null)
        {
            return Unauthorized(new
            {
                message =
                    "User identity could not be determined."
            });
        }

        try
        {
            var payment = initiateDto.PaymentMethod ==
                PaymentMethod.ESewa
                ? await _paymentService.InitiateEsewaPaymentAsync(
                    initiateDto, customerId)
                : new PaymentInitiationDto
                {
                    Payment = await _paymentService.InitiatePaymentAsync(
                        initiateDto, customerId)
                };

            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    [AllowAnonymous]
    [AcceptVerbs("GET", "POST")]
    [Route("esewa/success")]
    public async Task<IActionResult> EsewaSuccess(
        [FromQuery] string? data)
    {
        var encodedData = data;

        if (string.IsNullOrWhiteSpace(encodedData) &&
            Request.HasFormContentType)
        {
            encodedData = Request.Form["data"].ToString();
        }

        if (string.IsNullOrWhiteSpace(encodedData))
        {
            return BadRequest(new { message = "eSewa callback data is missing." });
        }

        try
        {
            var payment = await _paymentService.ProcessEsewaCallbackAsync(
                encodedData);

            return RedirectToFrontend(
                _frontendSettings.PaymentSuccessPath,
                payment.OrderId,
                "success");
        }
        catch (InvalidOperationException ex)
        {
            return RedirectToFrontend(
                _frontendSettings.PaymentFailurePath,
                null,
                "invalid",
                ex.Message);
        }
    }

    [AllowAnonymous]
    [AcceptVerbs("GET", "POST")]
    [Route("esewa/failure")]
    public IActionResult EsewaFailure()
    {
        return RedirectToFrontend(
            _frontendSettings.PaymentFailurePath,
            null,
            "failed");
    }

    [HttpPost("esewa/status/{orderId:int}")]
    public async Task<ActionResult<PaymentDto>> CheckEsewaStatus(
        int orderId)
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == null)
        {
            return Unauthorized(new
            {
                message = "User identity could not be determined."
            });
        }

        try
        {
            return Ok(await _paymentService.CheckEsewaStatusAsync(
                orderId,
                customerId));
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new { message = ex.Message });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new { message = ex.Message });
        }
    }

    [HttpPost("verify")]
    public async Task<ActionResult<PaymentDto>>
        VerifyPayment(
            VerifyPaymentDto verifyDto)
    {
        var customerId =
            GetCurrentCustomerId();

        if (customerId == null)
        {
            return Unauthorized(new
            {
                message =
                    "User identity could not be determined."
            });
        }

        try
        {
            var payment =
                await _paymentService.VerifyPaymentAsync(
                    verifyDto,
                    customerId);

            return Ok(payment);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    private string? GetCurrentCustomerId()
    {
        return User.FindFirstValue(
            ClaimTypes.NameIdentifier);
    }

    private IActionResult RedirectToFrontend(
        string path,
        int? orderId,
        string status,
        string? message = null)
    {
        if (string.IsNullOrWhiteSpace(_frontendSettings.BaseUrl))
        {
            return Problem(
                "Frontend payment redirect is not configured.");
        }

        var query = new List<string>
        {
            $"status={Uri.EscapeDataString(status)}"
        };

        if (orderId.HasValue)
        {
            query.Add($"orderId={orderId.Value}");
        }

        if (!string.IsNullOrWhiteSpace(message))
        {
            query.Add($"message={Uri.EscapeDataString(message)}");
        }

        var redirectUrl =
            $"{_frontendSettings.BaseUrl.TrimEnd('/')}/" +
            $"{path.TrimStart('/')}?{string.Join('&', query)}";

        return Redirect(redirectUrl);
    }
}