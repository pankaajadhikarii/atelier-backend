using Atelier_backend.Models.DTOs.Fabric;

namespace Atelier_backend.Services;

public interface IFabricService
{
    Task<IReadOnlyList<FabricDto>> GetAllAsync(
        bool includeInactive = false);

    Task<FabricDto?> GetByIdAsync(
        int id,
        bool includeInactive = false);

    Task<FabricDto> CreateAsync(
        CreateFabricDto createDto);

    Task<FabricDto?> UpdateAsync(
        int id,
        UpdateFabricDto updateDto);

    Task<bool> DeleteAsync(int id);
}