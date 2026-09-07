using Atelier_backend.Models.DTOs.Garment;

namespace Atelier_backend.Services;

public interface IGarmentService
{
    Task<IReadOnlyList<GarmentDto>> GetAllAsync(bool includeInactive = false);
    Task<GarmentDto?> GetByIdAsync(int id, bool includeInactive = false);
    Task<GarmentDto> CreateAsync(CreateGarmentDto createDto);
    Task<GarmentDto?> UpdateAsync(int id, UpdateGarmentDto updateDto);
    Task<bool> DeleteAsync(int id);
}
