using System.Security.Claims;
using Atelier_backend.Models.DTOs.Order;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class OrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public OrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: /api/order
    [HttpGet]
    public async Task<ActionResult<IEnumerable<OrderSummaryDto>>>
        GetOrders()
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == null)
        {
            return Unauthorized(new
            {
                message = "User identity could not be determined."
            });
        }

        var orders = await _orderService
            .GetAllAsync(customerId);

        return Ok(orders);
    }

    // GET: /api/order/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>>
        GetOrder(int id)
    {
        var customerId = GetCurrentCustomerId();

        if (customerId == null)
        {
            return Unauthorized(new
            {
                message = "User identity could not be determined."
            });
        }

        var order = await _orderService
            .GetByIdAsync(id, customerId);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        return Ok(order);
    }

    // POST: /api/order
    [HttpPost]
    public async Task<ActionResult<OrderDto>>
        CreateOrder(CreateOrderDto createDto)
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
            var order = await _orderService
                .CreateAsync(
                    createDto,
                    customerId);

            return CreatedAtAction(
                nameof(GetOrder),
                new { id = order.Id },
                order);
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