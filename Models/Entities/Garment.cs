namespace Atelier_backend.Models.Entities;

public class Garment
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string RequiredMeasurementsJson { get; set; } = "[]";
    public decimal? BasePrice { get; set; }
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<Design> Designs { get; set; } = new List<Design>();
    public ICollection<MeasurementProfile> MeasurementProfiles { get; set; } = new List<MeasurementProfile>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
}
