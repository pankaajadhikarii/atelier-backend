using Atelier_backend.Models.DTOs.Fabric;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class FabricController : ControllerBase
{
    private readonly IFabricService _fabricService;

    public FabricController(IFabricService fabricService)
    {
        _fabricService = fabricService;
    }

    // GET: /api/fabric
    // Public endpoint - returns only active fabrics.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<FabricDto>>> GetAllFabrics()
    {
        var fabrics = await _fabricService.GetAllAsync();

        return Ok(fabrics);
    }

    // GET: /api/fabric/{id}
    // Public endpoint - returns only active fabrics.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<FabricDto>> GetFabric(int id)
    {
        var fabric = await _fabricService.GetByIdAsync(id);

        if (fabric == null)
        {
            return NotFound(new
            {
                message = "Fabric not found."
            });
        }

        return Ok(fabric);
    }

    // POST: /api/fabric
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<FabricDto>> CreateFabric(
        CreateFabricDto createDto)
    {
        try
        {
            var fabric = await _fabricService.CreateAsync(createDto);

            return CreatedAtAction(
                nameof(GetFabric),
                new { id = fabric.Id },
                fabric);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: /api/fabric/{id}
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<FabricDto>> UpdateFabric(
        int id,
        UpdateFabricDto updateDto)
    {
        try
        {
            var fabric = await _fabricService.UpdateAsync(
                id,
                updateDto);

            if (fabric == null)
            {
                return NotFound(new
                {
                    message = "Fabric not found."
                });
            }

            return Ok(fabric);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: /api/fabric/{id}
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteFabric(int id)
    {
        var deleted = await _fabricService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Fabric not found."
            });
        }

        return Ok(new
        {
            message = "Fabric deleted successfully."
        });
    }
}