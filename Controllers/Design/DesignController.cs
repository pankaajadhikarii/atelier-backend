using Atelier_backend.Models.DTOs.Design;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
public class DesignController : ControllerBase
{
    private readonly IDesignService _designService;

    public DesignController(IDesignService designService)
    {
        _designService = designService;
    }

    // GET: /api/design
    // Public endpoint - returns only active designs.
    [HttpGet]
    public async Task<ActionResult<IEnumerable<DesignDto>>> GetAllDesigns()
    {
        var designs = await _designService.GetAllAsync();

        return Ok(designs);
    }

    // GET: /api/design/{id}
    // Public endpoint - returns only active designs.
    [HttpGet("{id:int}")]
    public async Task<ActionResult<DesignDto>> GetDesign(int id)
    {
        var design = await _designService.GetByIdAsync(id);

        if (design == null)
        {
            return NotFound(new
            {
                message = "Design not found."
            });
        }

        return Ok(design);
    }

    // POST: /api/design
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpPost]
    public async Task<ActionResult<DesignDto>> CreateDesign(
        CreateDesignDto createDto)
    {
        try
        {
            var design = await _designService.CreateAsync(createDto);

            return CreatedAtAction(
                nameof(GetDesign),
                new { id = design.Id },
                design);
        }
        catch (KeyNotFoundException ex)
        {
            return NotFound(new
            {
                message = ex.Message
            });
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // PUT: /api/design/{id}
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpPut("{id:int}")]
    public async Task<ActionResult<DesignDto>> UpdateDesign(
        int id,
        UpdateDesignDto updateDto)
    {
        try
        {
            var design = await _designService.UpdateAsync(
                id,
                updateDto);

            if (design == null)
            {
                return NotFound(new
                {
                    message = "Design not found."
                });
            }

            return Ok(design);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: /api/design/{id}
    // Admin + JWT required.
    [Authorize(
        AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme,
        Roles = "Admin")]
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteDesign(int id)
    {
        var deleted = await _designService.DeleteAsync(id);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Design not found."
            });
        }

        return Ok(new
        {
            message = "Design deleted successfully."
        });
    }
}