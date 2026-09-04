namespace Atelier_backend.Models.Entities;

public class MeasurementProfile
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public int GarmentId { get; set; }
    public string MeasurementValuesJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

    // Navigation properties
    public ApplicationUser User { get; set; } = null!;
    public Garment Garment { get; set; } = null!;
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
