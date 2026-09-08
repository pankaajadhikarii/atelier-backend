using Atelier_backend.Models.DTOs.Order;

namespace Atelier_backend.Services;

public interface IOrderService
{
    // Customer
    Task<IReadOnlyList<OrderSummaryDto>> GetAllAsync(
        string customerId);

    Task<OrderDto?> GetByIdAsync(
        int id,
        string customerId);

    Task<OrderDto> CreateAsync(
        CreateOrderDto createDto,
        string customerId);

    Task<OrderDto?> CancelAsync(
        int id,
        string customerId);

    // Admin
    Task<IReadOnlyList<AdminOrderSummaryDto>>
        GetAllAdminAsync();

    Task<OrderDto?> GetByIdAdminAsync(
        int id);

    Task<OrderDto?> UpdateStatusAsync(
        int id,
        UpdateOrderStatusDto updateDto);
}