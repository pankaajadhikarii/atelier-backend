using System.Net.Mail;
using Atelier_backend.Models.Configuration;
using Atelier_backend.Models.Entities;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Options;

namespace Atelier_backend.Services;

public class AdminSetupService : IAdminSetupService
{
    private const string CustomerRole = "Customer";
    private const string AdminRole = "Admin";

    private readonly UserManager<ApplicationUser> _userManager;
    private readonly RoleManager<IdentityRole> _roleManager;
    private readonly IConfiguration _configuration;
    private readonly AdminSettings _settings;

    public AdminSetupService(
        UserManager<ApplicationUser> userManager,
        RoleManager<IdentityRole> roleManager,
        IConfiguration configuration,
        IOptions<AdminSettings> options)
    {
        _userManager = userManager;
        _roleManager = roleManager;
        _configuration = configuration;
        _settings = options.Value;
    }

    public async Task InitializeAsync()
    {
        ValidateConfiguration();

        await EnsureRoleAsync(CustomerRole);
        await EnsureRoleAsync(AdminRole);

        var admin = await _userManager.FindByEmailAsync(
            _settings.Email);

        if (admin == null)
        {
            await CreateAdminAsync();
            return;
        }

        await EnsureAdminIsActiveAsync(admin);
        await EnsureAdminRoleAsync(admin);
    }

    private async Task CreateAdminAsync()
    {
        var adminPassword = _configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Admin:Password must be configured through User Secrets.");
        }

        var admin = new ApplicationUser
        {
            UserName = _settings.Email,
            Email = _settings.Email,
            FullName = _settings.FullName,
            CreatedAt = DateTime.UtcNow,
            IsActive = true
        };

        var createResult = await _userManager.CreateAsync(
            admin,
            adminPassword);

        if (!createResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Admin account creation failed: " +
                $"{FormatErrors(createResult)}");
        }

        var roleResult = await _userManager.AddToRoleAsync(
            admin,
            AdminRole);

        if (!roleResult.Succeeded)
        {
            throw new InvalidOperationException(
                $"Admin role assignment failed: " +
                $"{FormatErrors(roleResult)}");
        }
    }

    private async Task EnsureAdminIsActiveAsync(
        ApplicationUser admin)
    {
        if (admin.IsActive)
        {
            return;
        }

        admin.IsActive = true;

        var result = await _userManager.UpdateAsync(admin);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Failed to activate Admin account: " +
                $"{FormatErrors(result)}");
        }
    }

    private async Task EnsureAdminRoleAsync(
        ApplicationUser admin)
    {
        var isAdmin = await _userManager.IsInRoleAsync(
            admin,
            AdminRole);

        if (isAdmin)
        {
            return;
        }

        var result = await _userManager.AddToRoleAsync(
            admin,
            AdminRole);

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Admin role assignment failed: " +
                $"{FormatErrors(result)}");
        }
    }

    private async Task EnsureRoleAsync(string roleName)
    {
        if (await _roleManager.RoleExistsAsync(roleName))
        {
            return;
        }

        var result = await _roleManager.CreateAsync(
            new IdentityRole(roleName));

        if (!result.Succeeded)
        {
            throw new InvalidOperationException(
                $"Role '{roleName}' creation failed: " +
                $"{FormatErrors(result)}");
        }
    }

    private void ValidateConfiguration()
    {
        if (string.IsNullOrWhiteSpace(_settings.Email))
        {
            throw new InvalidOperationException(
                "Admin:Email must be configured.");
        }

        if (!IsValidEmail(_settings.Email))
        {
            throw new InvalidOperationException(
                "Admin:Email must be a valid email address.");
        }

        if (string.IsNullOrWhiteSpace(_settings.FullName))
        {
            throw new InvalidOperationException(
                "Admin:FullName must be configured.");
        }

        var adminPassword = _configuration["Admin:Password"];

        if (string.IsNullOrWhiteSpace(adminPassword))
        {
            throw new InvalidOperationException(
                "Admin:Password must be configured through User Secrets.");
        }
    }

    private static bool IsValidEmail(string email)
    {
        try
        {
            var mailAddress = new MailAddress(email);

            return string.Equals(
                mailAddress.Address,
                email,
                StringComparison.OrdinalIgnoreCase);
        }
        catch (FormatException)
        {
            return false;
        }
    }

    private static string FormatErrors(IdentityResult result)
    {
        return string.Join(
            ", ",
            result.Errors.Select(error => error.Description));
    }
}