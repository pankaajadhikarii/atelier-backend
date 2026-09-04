using Atelier_backend.Models.DTOs.Design;

namespace Atelier_backend.Models.DTOs.Garment;

public class GarmentDto
{
    public int Id { get; set; }
    public string Name { get; set; } = string.Empty;
    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }
    public string RequiredMeasurementsJson { get; set; } = "[]";
    public decimal? BasePrice { get; set; }
    public bool IsActive { get; set; }

    public List<DesignDto> Designs { get; set; } = new List<DesignDto>();
}
