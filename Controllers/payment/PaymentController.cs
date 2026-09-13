using System.Security.Claims;
using Atelier_backend.Models.DTOs.Payment;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes =
        JwtBearerDefaults.AuthenticationScheme)]
public class PaymentController : ControllerBase
{
    private readonly IPaymentService _paymentService;

    public PaymentController(
        IPaymentService paymentService)
    {
        _paymentService = paymentService;
    }

    [HttpPost("initiate")]
    public async Task<ActionResult<PaymentDto>>
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
            var payment =
                await _paymentService.InitiatePaymentAsync(
                    initiateDto,
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
}