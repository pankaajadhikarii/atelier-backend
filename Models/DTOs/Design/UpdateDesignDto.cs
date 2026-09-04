using System.ComponentModel.DataAnnotations;

namespace Atelier_backend.Models.DTOs.Design;

public class UpdateDesignDto
{
    [Required(ErrorMessage = "Design name is required.")]
    [StringLength(100)]
    public string Name { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? ImageUrl { get; set; }

    [Required]
    [Range(0, 100000.00, ErrorMessage = "Tailoring price cannot be negative.")]
    public decimal TailoringPrice { get; set; }

    public bool IsActive { get; set; } = true;
}
