using System.Security.Claims;
using Atelier_backend.Models.DTOs.Measurement;
using Atelier_backend.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace Atelier_backend.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(
    AuthenticationSchemes = JwtBearerDefaults.AuthenticationScheme)]
public class MeasurementProfileController : ControllerBase
{
    private readonly IMeasurementProfileService _measurementProfileService;

    public MeasurementProfileController(
        IMeasurementProfileService measurementProfileService)
    {
        _measurementProfileService = measurementProfileService;
    }

    // GET: /api/measurementprofile
    [HttpGet]
    public async Task<ActionResult<IEnumerable<MeasurementProfileDto>>>
        GetAllProfiles()
    {
        var userId = GetCurrentUserId();

        var profiles = await _measurementProfileService
            .GetAllAsync(userId);

        return Ok(profiles);
    }

    // GET: /api/measurementprofile/{id}
    [HttpGet("{id:int}")]
    public async Task<ActionResult<MeasurementProfileDto>>
        GetProfile(int id)
    {
        var userId = GetCurrentUserId();

        var profile = await _measurementProfileService
            .GetByIdAsync(id, userId);

        if (profile == null)
        {
            return NotFound(new
            {
                message = "Measurement profile not found."
            });
        }

        return Ok(profile);
    }

    // POST: /api/measurementprofile
    [HttpPost]
    public async Task<ActionResult<MeasurementProfileDto>>
        CreateProfile(
            CreateMeasurementProfileDto createDto)
    {
        var userId = GetCurrentUserId();

        try
        {
            var profile = await _measurementProfileService
                .CreateAsync(createDto, userId);

            return CreatedAtAction(
                nameof(GetProfile),
                new { id = profile.Id },
                profile);
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

    // PUT: /api/measurementprofile/{id}
    [HttpPut("{id:int}")]
    public async Task<ActionResult<MeasurementProfileDto>>
        UpdateProfile(
            int id,
            UpdateMeasurementProfileDto updateDto)
    {
        var userId = GetCurrentUserId();

        try
        {
            var profile = await _measurementProfileService
                .UpdateAsync(id, updateDto, userId);

            if (profile == null)
            {
                return NotFound(new
                {
                    message = "Measurement profile not found."
                });
            }

            return Ok(profile);
        }
        catch (InvalidOperationException ex)
        {
            return Conflict(new
            {
                message = ex.Message
            });
        }
    }

    // DELETE: /api/measurementprofile/{id}
    [HttpDelete("{id:int}")]
    public async Task<IActionResult> DeleteProfile(int id)
    {
        var userId = GetCurrentUserId();

        var deleted = await _measurementProfileService
            .DeleteAsync(id, userId);

        if (!deleted)
        {
            return NotFound(new
            {
                message = "Measurement profile not found."
            });
        }

        return Ok(new
        {
            message = "Measurement profile deleted successfully."
        });
    }

    private string GetCurrentUserId()
    {
        return User.FindFirstValue(ClaimTypes.NameIdentifier)
            ?? throw new UnauthorizedAccessException(
                "User ID was not found in the token.");
    }
}