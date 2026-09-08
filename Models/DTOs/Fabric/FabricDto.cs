namespace Atelier_backend.Models.DTOs.Fabric;

public class FabricDto
{
    public int Id { get; set; }

    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    public decimal Price { get; set; }

    public string? Color { get; set; }

    public decimal AvailableQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; }
}