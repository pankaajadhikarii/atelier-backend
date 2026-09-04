using System.ComponentModel.DataAnnotations;

namespace Atelier_backend.Models.DTOs.Garment;

public class CreateGarmentDto
{
    [Required(ErrorMessage = "Garment name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }
    public string? Description { get; set; }
    public string? IconUrl { get; set; }

    [Required(ErrorMessage = "Required measurements definition is required.")]
    public string RequiredMeasurementsJson { get; set; } = "[]";

    public decimal? BasePrice { get; set; }
}
