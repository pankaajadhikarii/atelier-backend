namespace Atelier_backend.Models.Entities;

public class Fabric
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public decimal Price { get; set; }
    public string? Color { get; set; }
    public decimal AvailableQuantity { get; set; }
    public string? ImageUrl { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
