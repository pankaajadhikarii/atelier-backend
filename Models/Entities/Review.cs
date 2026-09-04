namespace Atelier_backend.Models.Entities;

public class Review
{
    public int Id { get; set; }
    public int OrderId { get; set; }
    public string CustomerId { get; set; } = string.Empty;
    public int Rating { get; set; }
    public string? Comment { get; set; }
    public string? AdminResponse { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public Order Order { get; set; } = null!;
    public ApplicationUser Customer { get; set; } = null!;
}
