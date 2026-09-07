using Atelier_backend.Data;
using Atelier_backend.Models.DTOs.Design;
using Atelier_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Services;

public class DesignService : IDesignService
{
    private readonly ApplicationDbContext _dbContext;

    public DesignService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<DesignDto>> GetAllAsync(
        bool includeInactive = false)
    {
        var query = _dbContext.Designs
            .AsNoTracking()
            .Include(d => d.Garment)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive);
        }

        var designs = await query
            .OrderBy(d => d.Garment.Name)
            .ThenBy(d => d.Name)
            .ToListAsync();

        return designs
            .Select(MapToDto)
            .ToList();
    }

    public async Task<DesignDto?> GetByIdAsync(
        int id,
        bool includeInactive = false)
    {
        var query = _dbContext.Designs
            .AsNoTracking()
            .Include(d => d.Garment)
            .Where(d => d.Id == id);

        if (!includeInactive)
        {
            query = query.Where(d => d.IsActive);
        }

        var design = await query
            .SingleOrDefaultAsync();

        return design == null
            ? null
            : MapToDto(design);
    }

    public async Task<DesignDto> CreateAsync(
        CreateDesignDto createDto)
    {
        var normalizedName = createDto.Name.Trim();

        ValidateName(normalizedName);
        ValidatePrice(createDto.TailoringPrice);

        var garment = await _dbContext.Garments
            .SingleOrDefaultAsync(g =>
                g.Id == createDto.GarmentId &&
                g.IsActive);

        if (garment == null)
        {
            throw new KeyNotFoundException(
                "The selected garment does not exist or is inactive.");
        }

        var alreadyExists = await _dbContext.Designs
            .AnyAsync(d =>
                d.GarmentId == createDto.GarmentId &&
                EF.Functions.ILike(
                    d.Name,
                    normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A design with this name already exists for the selected garment.");
        }

        var design = new Design
        {
            GarmentId = garment.Id,
            Name = normalizedName,
            Description = NormalizeOptional(
                createDto.Description),
            ImageUrl = NormalizeOptional(
                createDto.ImageUrl),
            TailoringPrice = createDto.TailoringPrice,
            IsActive = true,
            Garment = garment
        };

        _dbContext.Designs.Add(design);

        await _dbContext.SaveChangesAsync();

        return MapToDto(design);
    }

    public async Task<DesignDto?> UpdateAsync(
        int id,
        UpdateDesignDto updateDto)
    {
        var normalizedName = updateDto.Name.Trim();

        ValidateName(normalizedName);
        ValidatePrice(updateDto.TailoringPrice);

        var design = await _dbContext.Designs
            .Include(d => d.Garment)
            .SingleOrDefaultAsync(d => d.Id == id);

        if (design == null)
        {
            return null;
        }

        var alreadyExists = await _dbContext.Designs
            .AnyAsync(d =>
                d.Id != id &&
                d.GarmentId == design.GarmentId &&
                EF.Functions.ILike(
                    d.Name,
                    normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A design with this name already exists for the selected garment.");
        }

        design.Name = normalizedName;
        design.Description = NormalizeOptional(
            updateDto.Description);
        design.ImageUrl = NormalizeOptional(
            updateDto.ImageUrl);
        design.TailoringPrice = updateDto.TailoringPrice;
        design.IsActive = updateDto.IsActive;

        await _dbContext.SaveChangesAsync();

        return MapToDto(design);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var design = await _dbContext.Designs
            .SingleOrDefaultAsync(d => d.Id == id);

        if (design == null)
        {
            return false;
        }

        // Soft delete
        design.IsActive = false;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static DesignDto MapToDto(Design design)
    {
        return new DesignDto
        {
            Id = design.Id,
            GarmentId = design.GarmentId,
            GarmentName = design.Garment?.Name ?? string.Empty,
            Name = design.Name,
            Description = design.Description,
            ImageUrl = design.ImageUrl,
            TailoringPrice = design.TailoringPrice,
            IsActive = design.IsActive
        };
    }

    private static void ValidateName(string name)
    {
        if (string.IsNullOrWhiteSpace(name))
        {
            throw new InvalidOperationException(
                "Design name is required.");
        }
    }

    private static void ValidatePrice(decimal price)
    {
        if (price < 0)
        {
            throw new InvalidOperationException(
                "Tailoring price cannot be negative.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}