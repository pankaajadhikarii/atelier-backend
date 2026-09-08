using System.Text.Json;
using Atelier_backend.Data;
using Atelier_backend.Models.DTOs.Measurement;
using Atelier_backend.Models.Entities;
using Microsoft.EntityFrameworkCore;

namespace Atelier_backend.Services;

public class MeasurementProfileService : IMeasurementProfileService
{
    private readonly ApplicationDbContext _dbContext;

    public MeasurementProfileService(ApplicationDbContext dbContext)
    {
        _dbContext = dbContext;
    }

    public async Task<IReadOnlyList<MeasurementProfileDto>> GetAllAsync(
        string userId)
    {
        var profiles = await _dbContext.MeasurementProfiles
            .AsNoTracking()
            .Include(profile => profile.Garment)
            .Where(profile => profile.UserId == userId)
            .OrderBy(profile => profile.ProfileName)
            .ToListAsync();

        return profiles
            .Select(MapToDto)
            .ToList();
    }

    public async Task<MeasurementProfileDto?> GetByIdAsync(
        int id,
        string userId)
    {
        var profile = await _dbContext.MeasurementProfiles
            .AsNoTracking()
            .Include(profile => profile.Garment)
            .SingleOrDefaultAsync(profile =>
                profile.Id == id &&
                profile.UserId == userId);

        return profile == null
            ? null
            : MapToDto(profile);
    }

    public async Task<MeasurementProfileDto> CreateAsync(
        CreateMeasurementProfileDto createDto,
        string userId)
    {
        var normalizedName = createDto.ProfileName.Trim();
        var measurementValuesJson =
            createDto.MeasurementValuesJson.Trim();

        ValidateProfileName(normalizedName);
        ValidateMeasurementValues(measurementValuesJson);

        var garment = await _dbContext.Garments
            .SingleOrDefaultAsync(g =>
                g.Id == createDto.GarmentId &&
                g.IsActive);

        if (garment == null)
        {
            throw new KeyNotFoundException(
                "The selected garment does not exist or is inactive.");
        }

        var alreadyExists = await _dbContext.MeasurementProfiles
            .AnyAsync(profile =>
                profile.UserId == userId &&
                profile.GarmentId == createDto.GarmentId &&
                EF.Functions.ILike(
                    profile.ProfileName,
                    normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A measurement profile with this name already exists for the selected garment.");
        }

        var now = DateTime.UtcNow;

        var profile = new MeasurementProfile
        {
            UserId = userId,
            GarmentId = garment.Id,
            ProfileName = normalizedName,
            MeasurementValuesJson = measurementValuesJson,
            CreatedAt = now,
            UpdatedAt = now,
            Garment = garment
        };

        _dbContext.MeasurementProfiles.Add(profile);

        await _dbContext.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<MeasurementProfileDto?> UpdateAsync(
        int id,
        UpdateMeasurementProfileDto updateDto,
        string userId)
    {
        var normalizedName = updateDto.ProfileName.Trim();
        var measurementValuesJson =
            updateDto.MeasurementValuesJson.Trim();

        ValidateProfileName(normalizedName);
        ValidateMeasurementValues(measurementValuesJson);

        var profile = await _dbContext.MeasurementProfiles
            .Include(measurementProfile => measurementProfile.Garment)
            .SingleOrDefaultAsync(measurementProfile =>
                measurementProfile.Id == id &&
                measurementProfile.UserId == userId);

        if (profile == null)
        {
            return null;
        }

        var alreadyExists = await _dbContext.MeasurementProfiles
            .AnyAsync(measurementProfile =>
                measurementProfile.Id != id &&
                measurementProfile.UserId == userId &&
                measurementProfile.GarmentId == profile.GarmentId &&
                EF.Functions.ILike(
                    measurementProfile.ProfileName,
                    normalizedName));

        if (alreadyExists)
        {
            throw new InvalidOperationException(
                "A measurement profile with this name already exists for the selected garment.");
        }

        profile.ProfileName = normalizedName;
        profile.MeasurementValuesJson = measurementValuesJson;
        profile.UpdatedAt = DateTime.UtcNow;

        await _dbContext.SaveChangesAsync();

        return MapToDto(profile);
    }

    public async Task<bool> DeleteAsync(
        int id,
        string userId)
    {
        var profile = await _dbContext.MeasurementProfiles
            .SingleOrDefaultAsync(measurementProfile =>
                measurementProfile.Id == id &&
                measurementProfile.UserId == userId);

        if (profile == null)
        {
            return false;
        }

        _dbContext.MeasurementProfiles.Remove(profile);

        await _dbContext.SaveChangesAsync();

        return true;
    }

    private static MeasurementProfileDto MapToDto(
        MeasurementProfile profile)
    {
        return new MeasurementProfileDto
        {
            Id = profile.Id,
            UserId = profile.UserId,
            ProfileName = profile.ProfileName,
            GarmentId = profile.GarmentId,
            GarmentName = profile.Garment?.Name ?? string.Empty,
            MeasurementValuesJson = profile.MeasurementValuesJson,
            CreatedAt = profile.CreatedAt,
            UpdatedAt = profile.UpdatedAt
        };
    }

    private static void ValidateProfileName(
        string profileName)
    {
        if (string.IsNullOrWhiteSpace(profileName))
        {
            throw new InvalidOperationException(
                "Profile name is required.");
        }
    }

    private static void ValidateMeasurementValues(
        string value)
    {
        if (string.IsNullOrWhiteSpace(value))
        {
            throw new InvalidOperationException(
                "Measurement values JSON is required.");
        }

        try
        {
            using var document = JsonDocument.Parse(value);

            if (document.RootElement.ValueKind !=
                JsonValueKind.Object)
            {
                throw new InvalidOperationException(
                    "Measurement values must be a JSON object.");
            }
        }
        catch (JsonException)
        {
            throw new InvalidOperationException(
                "Measurement values must contain valid JSON.");
        }
    }
}