namespace Atelier_backend.Models.Entities;

public class Design
{
    public int Id { get; set; }
    public int GarmentId { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal TailoringPrice { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public Garment Garment { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
