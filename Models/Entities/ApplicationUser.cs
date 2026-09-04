using Microsoft.AspNetCore.Identity;

namespace Atelier_backend.Models.Entities;

public class ApplicationUser : IdentityUser
{
    public string FullName { get; set; } = string.Empty;
    public string? Address { get; set; }
    public string? ProfileImageUrl { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public bool IsActive { get; set; } = true;

    // Navigation properties
    public ICollection<MeasurementProfile> MeasurementProfiles { get; set; } = new List<MeasurementProfile>();
    public ICollection<Order> Orders { get; set; } = new List<Order>();
    public ICollection<Payment> Payments { get; set; } = new List<Payment>();
    public ICollection<Review> Reviews { get; set; } = new List<Review>();
}
