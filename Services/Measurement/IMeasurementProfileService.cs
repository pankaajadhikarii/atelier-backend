using Atelier_backend.Models.DTOs.Measurement;

namespace Atelier_backend.Services;

public interface IMeasurementProfileService
{
    Task<IReadOnlyList<MeasurementProfileDto>> GetAllAsync(
        string userId);

    Task<MeasurementProfileDto?> GetByIdAsync(
        int id,
        string userId);

    Task<MeasurementProfileDto> CreateAsync(
        CreateMeasurementProfileDto createDto,
        string userId);

    Task<MeasurementProfileDto?> UpdateAsync(
        int id,
        UpdateMeasurementProfileDto updateDto,
        string userId);

    Task<bool> DeleteAsync(
        int id,
        string userId);
}
