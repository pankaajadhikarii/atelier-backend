using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using Atelier_backend.Models.DTOs.Auth;
using Atelier_backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.IdentityModel.Tokens;

namespace Atelier_backend.Services;

public class AuthService : IAuthService
{
    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;

    public AuthService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
    }

    public async Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto)
    {
        var existingUser = await _userManager.FindByEmailAsync(registerDto.Email);

        if (existingUser != null)
        {
            throw new InvalidOperationException(
                "User with this email address already exists.");
        }

        var user = new ApplicationUser
        {
            UserName = registerDto.Email,
            Email = registerDto.Email,
            FullName = registerDto.FullName,
            Address = registerDto.Address,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var result = await _userManager.CreateAsync(
            user,
            registerDto.Password);

        if (!result.Succeeded)
        {
            var errors = string.Join(
                ", ",
                result.Errors.Select(e => e.Description));

            throw new InvalidOperationException(
                $"Registration failed: {errors}");
        }

        // Ensure the Customer role exists.
        if (!await _roleManager.RoleExistsAsync("Customer"))
        {
            var roleResult = await _roleManager.CreateAsync(
                new IdentityRole("Customer"));

            if (!roleResult.Succeeded)
            {
                throw new InvalidOperationException(
                    "Failed to create Customer role.");
            }
        }

        // Every newly registered user is a Customer.
        var roleAssignmentResult = await _userManager.AddToRoleAsync(
            user,
            "Customer");

        if (!roleAssignmentResult.Succeeded)
        {
            throw new InvalidOperationException(
                "Failed to assign Customer role.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return GenerateJwtToken(user, roles);
    }

    public async Task<AuthResponseDto> LoginAsync(LoginDto loginDto)
    {
        var user = await _userManager.FindByEmailAsync(loginDto.Email);

        if (user == null || !user.IsActive)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var isPasswordValid = await _userManager.CheckPasswordAsync(
            user,
            loginDto.Password);

        if (!isPasswordValid)
        {
            throw new UnauthorizedAccessException(
                "Invalid email or password.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return GenerateJwtToken(user, roles);
    }

    public async Task<UserProfileDto> GetUserProfileAsync(string userId)
    {
        var user = await _userManager.FindByIdAsync(userId);

        if (user == null || !user.IsActive)
        {
            throw new KeyNotFoundException(
                "User profile not found.");
        }

        var roles = await _userManager.GetRolesAsync(user);

        return new UserProfileDto
        {
            Id = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            PhoneNumber = user.PhoneNumber,
            Address = user.Address,
            ProfileImageUrl = user.ProfileImageUrl,
            CreatedAt = user.CreatedAt,
            Roles = roles
        };
    }

    private AuthResponseDto GenerateJwtToken(
        ApplicationUser user,
        IList<string> roles)
    {
        var jwtSecret = _configuration["Jwt:Secret"]
            ?? throw new InvalidOperationException(
                "Jwt:Secret is not configured.");

        var jwtIssuer = _configuration["Jwt:Issuer"]
            ?? throw new InvalidOperationException(
                "Jwt:Issuer is not configured.");

        var jwtAudience = _configuration["Jwt:Audience"]
            ?? throw new InvalidOperationException(
                "Jwt:Audience is not configured.");

        var expirationMinutes = _configuration.GetValue<int>(
            "Jwt:ExpirationMinutes",
            60);

        var authClaims = new List<Claim>
        {
            new Claim(
                ClaimTypes.NameIdentifier,
                user.Id),

            new Claim(
                ClaimTypes.Email,
                user.Email ?? string.Empty),

            new Claim(
                ClaimTypes.Name,
                user.FullName),

            new Claim(
                JwtRegisteredClaimNames.Jti,
                Guid.NewGuid().ToString())
        };

        foreach (var role in roles)
        {
            authClaims.Add(
                new Claim(ClaimTypes.Role, role));
        }

        var authSigningKey = new SymmetricSecurityKey(
            Encoding.UTF8.GetBytes(jwtSecret));

        var expiration = DateTime.UtcNow.AddMinutes(
            expirationMinutes);

        var token = new JwtSecurityToken(
            issuer: jwtIssuer,
            audience: jwtAudience,
            expires: expiration,
            claims: authClaims,
            signingCredentials: new SigningCredentials(
                authSigningKey,
                SecurityAlgorithms.HmacSha256)
        );

        return new AuthResponseDto
        {
            Token = new JwtSecurityTokenHandler().WriteToken(token),
            Expiration = expiration,
            UserId = user.Id,
            FullName = user.FullName,
            Email = user.Email ?? string.Empty,
            Roles = roles
        };
    }
}