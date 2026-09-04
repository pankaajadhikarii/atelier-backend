using System.ComponentModel.DataAnnotations;

namespace Atelier_backend.Models.DTOs.Measurement;

public class CreateMeasurementProfileDto
{
    [Required(ErrorMessage = "Profile name is required.")]
    [StringLength(100)]
    public string ProfileName { get; set; } = string.Empty;

    [Required]
    public int GarmentId { get; set; }

    [Required(ErrorMessage = "Measurement values JSON is required.")]
    public string MeasurementValuesJson { get; set; } = "{}";
}
