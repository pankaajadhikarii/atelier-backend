namespace Atelier_backend.Models.DTOs.Shop;

public class ShopProfileDto
{
    public int Id { get; set; }

    public string BusinessName { get; set; } = string.Empty;

    public string? Description { get; set; }

    public string? Location { get; set; }

    public string? ContactPhone { get; set; }

    public string? ContactEmail { get; set; }

    public string? OpeningHours { get; set; }

    public decimal Rating { get; set; }

    public string? LogoUrl { get; set; }

    public string? CoverImageUrl { get; set; }
}