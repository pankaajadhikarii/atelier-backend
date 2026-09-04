namespace Atelier_backend.Models.DTOs.Review;

public class ReviewDto
{
    public int Id { get; set; }

    public int OrderId { get; set; }

    public string CustomerId { get; set; } = string.Empty;
    public string CustomerName { get; set; } = string.Empty;

    public int Rating { get; set; }
    public string? Comment { get; set; }

    public string? AdminResponse { get; set; }

    public DateTime CreatedAt { get; set; }
}