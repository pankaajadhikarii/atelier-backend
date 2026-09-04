using Atelier_backend.Models.Entities;
using Atelier_backend.Models.Enums;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Data;

public class ApplicationDbContext : IdentityDbContext<ApplicationUser>
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
        : base(options)
    {
    }

    public DbSet<ShopProfile> ShopProfiles => Set<ShopProfile>();
    public DbSet<Garment> Garments => Set<Garment>();
    public DbSet<Fabric> Fabrics => Set<Fabric>();
    public DbSet<Design> Designs => Set<Design>();
    public DbSet<MeasurementProfile> MeasurementProfiles => Set<MeasurementProfile>();
    public DbSet<Order> Orders => Set<Order>();
    public DbSet<OrderStatusHistory> OrderStatusHistories => Set<OrderStatusHistory>();
    public DbSet<Payment> Payments => Set<Payment>();
    public DbSet<Review> Reviews => Set<Review>();

    protected override void OnModelCreating(ModelBuilder builder)
    {
        base.OnModelCreating(builder);

        // --- JSONB Column Configurations ---
        builder.Entity<Garment>()
            .Property(g => g.RequiredMeasurementsJson)
            .HasColumnType("jsonb");

        builder.Entity<MeasurementProfile>()
            .Property(m => m.MeasurementValuesJson)
            .HasColumnType("jsonb");

        builder.Entity<Order>()
            .Property(o => o.MeasurementSnapshotJson)
            .HasColumnType("jsonb");

        builder.Entity<Order>()
            .Property(o => o.CustomizationDetailsJson)
            .HasColumnType("jsonb");

        builder.Entity<Payment>()
            .Property(p => p.GatewayResponseRaw)
            .HasColumnType("jsonb");

        // --- Decimal Precision Configurations ---
        builder.Entity<ShopProfile>()
            .Property(s => s.Rating)
            .HasPrecision(3, 2);

        builder.Entity<Garment>()
            .Property(g => g.BasePrice)
            .HasPrecision(10, 2);

        builder.Entity<Fabric>()
            .Property(f => f.Price)
            .HasPrecision(10, 2);

        builder.Entity<Fabric>()
            .Property(f => f.AvailableQuantity)
            .HasPrecision(10, 2);

        builder.Entity<Design>()
            .Property(d => d.TailoringPrice)
            .HasPrecision(10, 2);

        builder.Entity<Order>()
            .Property(o => o.FabricQuantity)
            .HasPrecision(10, 2);

        builder.Entity<Order>()
            .Property(o => o.FabricPrice)
            .HasPrecision(10, 2);

        builder.Entity<Order>()
            .Property(o => o.TailoringPrice)
            .HasPrecision(10, 2);

        builder.Entity<Order>()
            .Property(o => o.CustomizationPrice)
            .HasPrecision(10, 2);

        builder.Entity<Order>()
            .Property(o => o.DeliveryFee)
            .HasPrecision(10, 2);

        builder.Entity<Order>()
            .Property(o => o.TotalAmount)
            .HasPrecision(10, 2);

        builder.Entity<Payment>()
            .Property(p => p.Amount)
            .HasPrecision(10, 2);

        // --- Enum String Conversions ---
        builder.Entity<Order>()
            .Property(o => o.Status)
            .HasConversion<string>();

        builder.Entity<Order>()
            .Property(o => o.PaymentStatus)
            .HasConversion<string>();

        builder.Entity<Order>()
            .Property(o => o.PaymentMethod)
            .HasConversion<string>();

        builder.Entity<Order>()
            .Property(o => o.FitType)
            .HasConversion<string>();

        builder.Entity<OrderStatusHistory>()
            .Property(h => h.Status)
            .HasConversion<string>();

        builder.Entity<Payment>()
            .Property(p => p.PaymentMethod)
            .HasConversion<string>();

        builder.Entity<Payment>()
            .Property(p => p.Status)
            .HasConversion<string>();

        // --- Unique Indexes ---
        builder.Entity<Order>()
            .HasIndex(o => o.OrderNumber)
            .IsUnique();

        builder.Entity<Payment>()
            .HasIndex(p => p.OrderId)
            .IsUnique();

        builder.Entity<Review>()
            .HasIndex(r => r.OrderId)
            .IsUnique();

        // --- Check Constraints ---
        builder.Entity<Review>()
            .ToTable(t => t.HasCheckConstraint("CK_Review_Rating", "\"Rating\" >= 1 AND \"Rating\" <= 5"));

        // --- Relationships & Delete Behaviors ---
        builder.Entity<Order>()
            .HasOne(o => o.Customer)
            .WithMany(u => u.Orders)
            .HasForeignKey(o => o.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.Fabric)
            .WithMany(f => f.Orders)
            .HasForeignKey(o => o.FabricId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.Garment)
            .WithMany(g => g.Orders)
            .HasForeignKey(o => o.GarmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.Design)
            .WithMany(d => d.Orders)
            .HasForeignKey(o => o.DesignId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Order>()
            .HasOne(o => o.MeasurementProfile)
            .WithMany(m => m.Orders)
            .HasForeignKey(o => o.MeasurementProfileId)
            .OnDelete(DeleteBehavior.SetNull);

        builder.Entity<OrderStatusHistory>()
            .HasOne(h => h.Order)
            .WithMany(o => o.StatusHistory)
            .HasForeignKey(h => h.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Payment>()
            .HasOne(p => p.Order)
            .WithOne(o => o.Payment)
            .HasForeignKey<Payment>(p => p.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Payment>()
            .HasOne(p => p.Customer)
            .WithMany(u => u.Payments)
            .HasForeignKey(p => p.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Review>()
            .HasOne(r => r.Order)
            .WithOne(o => o.Review)
            .HasForeignKey<Review>(r => r.OrderId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<Review>()
            .HasOne(r => r.Customer)
            .WithMany(u => u.Reviews)
            .HasForeignKey(r => r.CustomerId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<MeasurementProfile>()
            .HasOne(m => m.User)
            .WithMany(u => u.MeasurementProfiles)
            .HasForeignKey(m => m.UserId)
            .OnDelete(DeleteBehavior.Cascade);

        builder.Entity<MeasurementProfile>()
            .HasOne(m => m.Garment)
            .WithMany(g => g.MeasurementProfiles)
            .HasForeignKey(m => m.GarmentId)
            .OnDelete(DeleteBehavior.Restrict);

        builder.Entity<Design>()
            .HasOne(d => d.Garment)
            .WithMany(g => g.Designs)
            .HasForeignKey(d => d.GarmentId)
            .OnDelete(DeleteBehavior.Cascade);
    }
}
