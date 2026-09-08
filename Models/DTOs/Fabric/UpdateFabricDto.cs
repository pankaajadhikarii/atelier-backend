using System.ComponentModel.DataAnnotations;

namespace Atelier_backend.Models.DTOs.Fabric;

public class UpdateFabricDto
{
    [Required(ErrorMessage = "Fabric name is required.")]
    [StringLength(
        100,
        ErrorMessage = "Fabric name cannot exceed 100 characters."
    )]
    public string Name { get; set; } = string.Empty;

    public string? Category { get; set; }

    public string? Description { get; set; }

    [Required]
    [Range(
        0.01,
        100000.00,
        ErrorMessage = "Price must be greater than 0."
    )]
    public decimal Price { get; set; }

    public string? Color { get; set; }

    [Required]
    [Range(
        0,
        10000.00,
        ErrorMessage = "Available quantity must be between 0 and 10000."
    )]
    public decimal AvailableQuantity { get; set; }

    public string? ImageUrl { get; set; }

    public bool IsActive { get; set; } = true;
}