namespace Atelier_backend.Models.DTOs.Measurement;

public class MeasurementProfileDto
{
    public int Id { get; set; }
    public string UserId { get; set; } = string.Empty;
    public string ProfileName { get; set; } = string.Empty;
    public int GarmentId { get; set; }
    public string GarmentName { get; set; } = string.Empty;
    public string MeasurementValuesJson { get; set; } = "{}";
    public DateTime CreatedAt { get; set; }
    public DateTime UpdatedAt { get; set; }
}
