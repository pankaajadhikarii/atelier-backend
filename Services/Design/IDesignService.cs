using Atelier_backend.Models.DTOs.Design;

namespace Atelier_backend.Services;

public interface IDesignService
{
    Task<IReadOnlyList<DesignDto>> GetAllAsync(
        bool includeInactive = false);

    Task<DesignDto?> GetByIdAsync(
        int id,
        bool includeInactive = false);

    Task<DesignDto> CreateAsync(
        CreateDesignDto createDto);

    Task<DesignDto?> UpdateAsync(
        int id,
        UpdateDesignDto updateDto);

    Task<bool> DeleteAsync(int id);
}