using System.ComponentModel.DataAnnotations;

namespace Atelier_backend.Models.DTOs.Review;

public class AdminReviewResponseDto
{
    [Required(ErrorMessage = "Response text is required.")]
    [StringLength(1000, ErrorMessage = "Response cannot exceed 1000 characters.")]
    public string AdminResponse { get; set; } = string.Empty;
}