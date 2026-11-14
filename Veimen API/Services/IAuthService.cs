using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IAuthService
{
    Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent);
    Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent);
    Task LogoutAsync(long userId, string refreshToken);
    Task<UserDto> GetMeAsync(long userId);
    Task<UpdateLanguageResponse> UpdateLanguageAsync(long userId, string language);
    Task ChangePasswordAsync(long userId, string currentPassword, string newPassword);
}