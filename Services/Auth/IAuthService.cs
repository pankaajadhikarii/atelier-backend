using Atelier_backend.Models.DTOs.Auth;

namespace Atelier_backend.Services;

public interface IAuthService
{
    Task<AuthResponseDto> RegisterAsync(RegisterDto registerDto);
    Task<AuthResponseDto> LoginAsync(LoginDto loginDto);
    Task<UserProfileDto> GetUserProfileAsync(string userId);
}
