using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.Entities;

public class OrderStatusHistory
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public OrderStatus Status { get; set; }
    public string? Notes { get; set; }
    public DateTime Timestamp { get; set; } = DateTime.UtcNow;

    // Navigation property
    public Order Order { get; set; } = null!;
}
