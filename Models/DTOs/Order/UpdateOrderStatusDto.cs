using System.ComponentModel.DataAnnotations;
using Atelier_backend.Models.Enums;

namespace Atelier_backend.Models.DTOs.Order;

public class UpdateOrderStatusDto
{
    [Required]
    public OrderStatus Status { get; set; }

    public string? RejectionReason { get; set; }

    public string? Notes { get; set; }
}