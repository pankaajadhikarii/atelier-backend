using Atelier_backend.Models.DTOs.Garment;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class GarmentController : ControllerBase
{
    private readonly IGarmentService _garmentService;

    public GarmentController(IGarmentService garmentService)
    {
        _garmentService = garmentService;
    }

    // GET: /api/garment
    // Public endpoint - returns only active garments.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<GarmentDto>>> GetAllGarments()
    {
        var garments = await _garmentService.GetAllAsync(
            includeInactive: false);

        return Ok(garments);
    }

    // GET: /api/garment/{id}
    // Public endpoint - returns only active garments.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<GarmentDto>> GetGarment(int id)
    {
        var garment = await _garmentService.GetByIdAsync(
            id,
            includeInactive: false);

        if (garment == null)
        {
            return NotFound(new
            {
                message = "Garment not found."
            });
        }

        return Ok(garment);
    }

    // POST: /api/garment
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<GarmentDto>> CreateGarment(
        CreateGarmentDto createDto)
    {
        try
        {
            var garment = await _garmentService.CreateAsync(createDto);

            return CreatedAtAction(
                nameof(GetGarment),
                new { id = garment.Id },
                garment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: /api/garment/{id}
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<GarmentDto>> UpdateGarment(
        int id,
        UpdateGarmentDto updateDto)
    {
        try
        {
            var garment = await _garmentService.UpdateAsync(
                id,
                updateDto);

            if (garment == null)
            {
                return NotFound(new
                {
                    message = "Garment not found."
                });
            }

            return Ok(garment);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: /api/garment/{id}
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteGarment(int id)
    {
        var deleted = await _garmentService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Garment not found."
            });
        }

        return NoContent();
    }
}