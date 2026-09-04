using System.ComponentModel.DataAnnotations;

namespace Atelier_backend.Models.DTOs.Shop;

public class UpdateShopProfileDto
{
    [Required(ErrorMessage = "Business name is required.")]
    [StringLength(150, ErrorMessage = "Business name cannot exceed 150 characters.")]
    public string BusinessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? ContactPhone { get; set; }

    [EmailAddress(ErrorMessage = "Invalid email address format.")]
    public string? ContactEmail { get; set; }

    public string? OpeningHours { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }
}