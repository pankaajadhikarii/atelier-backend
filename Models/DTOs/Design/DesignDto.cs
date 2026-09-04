namespace Atelier_backend.Models.DTOs.Design;

public class DesignDto
{
    public int Id { get; set; }
    public int GarmentId { get; set; }
    public string GarmentName { get; set; } = string.Empty;
    public string Name { get; set; } = string.Empty;
    public string? Description { get; set; }
    public string? ImageUrl { get; set; }
    public decimal TailoringPrice { get; set; }
    public bool IsActive { get; set; }
}
