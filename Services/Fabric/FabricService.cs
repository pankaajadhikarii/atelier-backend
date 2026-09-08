using Atelier_backend.Data;
using Atelier_backend.Models.DTOs.Fabric;
using Atelier_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Services;

public class FabricService : IFabricService
{
    private readonly ApplicationDbContext _dbContext;

    public FabricService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<FabricDto>> GetAllAsync(
        bool includeInactive = false)
    {
        var query = _dbContext.Fabrics
            .AsNoTracking()
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        var fabrics = await query
            .OrderBy(f => f.Name)
            .ToListAsync();

        return fabrics
            .Select(MapToDto)
            .ToList();
    }

    public async Task<FabricDto?> GetByIdAsync(
        int id,
        bool includeInactive = false)
    {
        var query = _dbContext.Fabrics
            .AsNoTracking()
            .Where(f => f.Id == id);

        if (!includeInactive)
        {
            query = query.Where(f => f.IsActive);
        }

        var fabric = await query
            .SingleOrDefaultAsync();

        return fabric == null
            ? null
            : MapToDto(fabric);
    }

    public async Task<FabricDto> CreateAsync(
        CreateFabricDto createDto)
    {
        var normalizedName = createDto.Name.Trim();

        ValidateName(normalizedName);
        ValidatePrice(createDto.Price);
        ValidateStock(createDto.AvailableQuantity);

        var alreadyExists = await _dbContext.Fabrics
            .AnyAsync(f =>
                EF.Functions.ILike(
                    f.Name,
                    normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A fabric with this name already exists.");
        }

        var fabric = new Fabric
        {
            Name = normalizedName,
            Category = NormalizeOptional(createDto.Category),
            Description = NormalizeOptional(createDto.Description),
            Price = createDto.Price,
            Color = NormalizeOptional(createDto.Color),
            AvailableQuantity = createDto.AvailableQuantity,
            ImageUrl = NormalizeOptional(createDto.ImageUrl),
            IsActive = true
        };

        _dbContext.Fabrics.Add(fabric);

        await _dbContext.SaveChangesAsync();

        return MapToDto(fabric);
    }

    public async Task<FabricDto?> UpdateAsync(
        int id,
        UpdateFabricDto updateDto)
    {
        var normalizedName = updateDto.Name.Trim();

        ValidateName(normalizedName);
        ValidatePrice(updateDto.Price);
        ValidateStock(updateDto.AvailableQuantity);

        var fabric = await _dbContext.Fabrics
            .SingleOrDefaultAsync(f => f.Id == id);

        if (fabric == null)
        {
            return null;
        }

        var alreadyExists = await _dbContext.Fabrics
            .AnyAsync(f =>
                f.Id != id &&
                EF.Functions.ILike(
                    f.Name,
                    normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A fabric with this name already exists.");
        }

        fabric.Name = normalizedName;
        fabric.Category = NormalizeOptional(updateDto.Category);
        fabric.Description = NormalizeOptional(updateDto.Description);
        fabric.Price = updateDto.Price;
        fabric.Color = NormalizeOptional(updateDto.Color);
        fabric.AvailableQuantity = updateDto.AvailableQuantity;
        fabric.ImageUrl = NormalizeOptional(updateDto.ImageUrl);
        fabric.IsActive = updateDto.IsActive;

        await _dbContext.SaveChangesAsync();

        return MapToDto(fabric);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var fabric = await _dbContext.Fabrics
            .SingleOrDefaultAsync(f => f.Id == id);

        if (fabric == null)
        {
            return false;
        }

        // Soft delete
        fabric.IsActive = false;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static FabricDto MapToDto(Fabric fabric)
    {
        return new FabricDto
        {
            Id = fabric.Id,
            Name = fabric.Name,
            Category = fabric.Category,
            Description = fabric.Description,
            Price = fabric.Price,
            Color = fabric.Color,
            AvailableQuantity = fabric.AvailableQuantity,
            ImageUrl = fabric.ImageUrl,
            IsActive = fabric.IsActive
        };
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Fabric name is required.");
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price <= 0)
        {
            throw new InvalidOperationException(
                "Price must be greater than 0.");
        }
    }

    private static void ValidateStock(decimal stock)
    {
        if (stock < 0)
        {
            throw new InvalidOperationException(
                "Available quantity cannot be negative.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}