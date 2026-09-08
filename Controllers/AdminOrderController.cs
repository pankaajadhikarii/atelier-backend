using Atelier_backend.Models.DTOs.Order;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/admin/order")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
    Roles = "Admin")]
public class AdminOrderController : ControllerBase
{
    private readonly IOrderService _orderService;

    public AdminOrderController(IOrderService orderService)
    {
        _orderService = orderService;
    }

    // GET: /api/admin/order
    [HttpGet]
    public async Task<ActionResult<
        IEnumerable<AdminOrderSummaryDto>>> GetAllOrders()
    {
        var orders =
            await _orderService.GetAllAdminAsync();

        return Ok(orders);
    }

    // GET: /api/admin/order/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<OrderDto>> GetOrder(
        int id)
    {
        var order =
            await _orderService.GetByIdAdminAsync(id);

        if (order == null)
        {
            return NotFound(new
            {
                message = "Order not found."
            });
        }

        return Ok(order);
    }

    // PATCH: /api/admin/order/{id}/status
    [HttpPatch("{id:int}/status")]
    public async Task<ActionResult<OrderDto>> UpdateStatus(
        int id,
        UpdateOrderStatusDto updateDto)
    {
        try
        {
            var order =
                await _orderService.UpdateStatusAsync(
                    id,
                    updateDto);

            if (order == null)
            {
                return NotFound(new
                {
                    message = "Order not found."
                });
            }

            return Ok(order);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }
}