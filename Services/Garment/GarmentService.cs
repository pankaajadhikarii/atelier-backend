using System.Text.Json;
using Atelier_backend.Data;
using Atelier_backend.Models.DTOs.Design;
using Atelier_backend.Models.DTOs.Garment;
using Atelier_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Services;

public class GarmentService : IGarmentService
{
    private readonly ApplicationDbContext _dbContext;

    public GarmentService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<GarmentDto>> GetAllAsync(
        bool includeInactive = false)
    {
        var query = _dbContext.Garments
            .AsNoTracking()
            .Include(g => g.Designs)
            .AsQueryable();

        if (!includeInactive)
        {
            query = query.Where(g => g.IsActive);
        }

        var garments = await query
            .OrderBy(g => g.Category)
            .ThenBy(g => g.Name)
            .ToListAsync();

        return garments
            .Select(MapToDto)
            .ToList();
    }

    public async Task<GarmentDto?> GetByIdAsync(
        int id,
        bool includeInactive = false)
    {
        var query = _dbContext.Garments
            .AsNoTracking()
            .Include(g => g.Designs)
            .Where(g => g.Id == id);

        if (!includeInactive)
        {
            query = query.Where(g => g.IsActive);
        }

        var garment = await query
            .SingleOrDefaultAsync();

        return garment == null
            ? null
            : MapToDto(garment);
    }

    public async Task<GarmentDto> CreateAsync(
        CreateGarmentDto createDto)
    {
        var normalizedName = createDto.Name.Trim();
        var measurementDefinition =
            createDto.RequiredMeasurementsJson.Trim();

        ValidateMeasurementDefinition(measurementDefinition);
        ValidatePrice(createDto.BasePrice);

        var alreadyExists = await _dbContext.Garments
            .AnyAsync(g =>
                EF.Functions.ILike(g.Name, normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A garment with this name already exists.");
        }

        var garment = new Garment
        {
            Name = normalizedName,
            Category = NormalizeOptional(createDto.Category),
            Description = NormalizeOptional(createDto.Description),
            IconUrl = NormalizeOptional(createDto.IconUrl),
            RequiredMeasurementsJson = measurementDefinition,
            BasePrice = createDto.BasePrice,
            IsActive = true
        };

        _dbContext.Garments.Add(garment);

        await _dbContext.SaveChangesAsync();

        return MapToDto(garment);
    }

    public async Task<GarmentDto?> UpdateAsync(
        int id,
        UpdateGarmentDto updateDto)
    {
        var normalizedName = updateDto.Name.Trim();
        var measurementDefinition =
            updateDto.RequiredMeasurementsJson.Trim();

        ValidateMeasurementDefinition(measurementDefinition);
        ValidatePrice(updateDto.BasePrice);

        var garment = await _dbContext.Garments
            .Include(g => g.Designs)
            .SingleOrDefaultAsync(g => g.Id == id);

        if (garment == null)
        {
            return null;
        }

        var alreadyExists = await _dbContext.Garments
            .AnyAsync(g =>
                g.Id != id &&
                EF.Functions.ILike(g.Name, normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A garment with this name already exists.");
        }

        garment.Name = normalizedName;
        garment.Category = NormalizeOptional(updateDto.Category);
        garment.Description = NormalizeOptional(updateDto.Description);
        garment.IconUrl = NormalizeOptional(updateDto.IconUrl);
        garment.RequiredMeasurementsJson = measurementDefinition;
        garment.BasePrice = updateDto.BasePrice;
        garment.IsActive = updateDto.IsActive;

        await _dbContext.SaveChangesAsync();

        return MapToDto(garment);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var garment = await _dbContext.Garments
            .SingleOrDefaultAsync(g => g.Id == id);

        if (garment == null)
        {
            return false;
        }

        // Soft delete
        garment.IsActive = false;

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static GarmentDto MapToDto(Garment garment)
    {
        return new GarmentDto
        {
            Id = garment.Id,
            Name = garment.Name,
            Category = garment.Category,
            Description = garment.Description,
            IconUrl = garment.IconUrl,
            RequiredMeasurementsJson =
                garment.RequiredMeasurementsJson,
            BasePrice = garment.BasePrice,
            IsActive = garment.IsActive,

            Designs = garment.Designs
                .Where(d => d.IsActive)
                .OrderBy(d => d.Name)
                .Select(d => new DesignDto
                {
                    Id = d.Id,
                    GarmentId = d.GarmentId,
                    GarmentName = garment.Name,
                    Name = d.Name,
                    Description = d.Description,
                    ImageUrl = d.ImageUrl,
                    TailoringPrice = d.TailoringPrice,
                    IsActive = d.IsActive
                })
                .ToList()
        };
    }

    private static void ValidateMeasurementDefinition(
        string value)
    {
        try
        {
            using var document = JsonDocument.Parse(value);

            if (document.RootElement.ValueKind !=
                JsonValueKind.Array)
            {
                throw new InvalidOperationException(
                    "Required measurements must be a JSON array.");
            }
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Required measurements must contain valid JSON.");
        }
    }

    private static void ValidatePrice(decimal? price)
    {
        if (price is < 0)
        {
            throw new InvalidOperationException(
                "Base price cannot be negative.");
        }
    }

    private static string? NormalizeOptional(string? value)
    {
        return string.IsNullOrWhiteSpace(value)
            ? null
            : value.Trim();
    }
}