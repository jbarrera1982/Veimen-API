using Veimen_API.Exceptions;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Repositories;
using Microsoft.AspNetCore.Http;

namespace Veimen_API.Services;

public class AuthService : IAuthService
{
    private readonly IUserRepository _userRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;
    private readonly IPermissionRepository _permissionRepository;
    private readonly TokenService _tokenService;

    public AuthService(
        IUserRepository userRepository,
        IRefreshTokenRepository refreshTokenRepository,
        IPermissionRepository permissionRepository,
        TokenService tokenService)
    {
        _userRepository = userRepository;
        _refreshTokenRepository = refreshTokenRepository;
        _permissionRepository = permissionRepository;
        _tokenService = tokenService;
    }

    public async Task<AuthResponse> LoginAsync(LoginRequest request, string? ipAddress, string? userAgent)
    {
        var identifier = request.Identifier.Trim();

        var user = await _userRepository.GetByIdentifierAsync(identifier);
        if (user is null || !BCrypt.Net.BCrypt.Verify(request.Password, user.PasswordHash))
        {
            throw new AuthException("Credenciales inválidas.", StatusCodes.Status401Unauthorized);
        }

        if (!user.Active)
        {
            throw new AuthException("El usuario está desactivado.", StatusCodes.Status403Forbidden);
        }

        await _userRepository.UpdateLastLoginAsync(user.UserId, DateTime.UtcNow);
        return await BuildAuthResponseAsync(user, ipAddress, userAgent);
    }

    public async Task<AuthResponse> RefreshAsync(string refreshToken, string? ipAddress, string? userAgent)
    {
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(TokenService.HashToken(refreshToken));
        if (stored is null || stored.RevokedAt is not null || stored.ExpiresAt < DateTime.UtcNow)
        {
            throw new AuthException("El token de refresco es inválido o ha expirado.", StatusCodes.Status401Unauthorized);
        }

        var user = await _userRepository.GetByIdAsync(stored.UserId);
        if (user is null || !user.Active)
        {
            throw new AuthException("El usuario ya no es válido.", StatusCodes.Status401Unauthorized);
        }

        var (newRefreshToken, newTokenHash, newTokenExpiresAt) = _tokenService.GenerateRefreshToken();

        await _refreshTokenRepository.RevokeAsync(stored.RefreshTokenId, newTokenHash);
        var response = await BuildAuthResponseAsync(user, ipAddress, userAgent, (newRefreshToken, newTokenHash, newTokenExpiresAt));

        return response;
    }

    public async Task LogoutAsync(long userId, string refreshToken)
    {
        var stored = await _refreshTokenRepository.GetByTokenHashAsync(TokenService.HashToken(refreshToken));
        if (stored is null || stored.UserId != userId)
        {
            return;
        }

        await _refreshTokenRepository.RevokeAsync(stored.RefreshTokenId, null);
    }

    public async Task<UserDto> GetMeAsync(long userId)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.Active)
        {
            throw new AuthException("El usuario no existe o está desactivado.", StatusCodes.Status401Unauthorized);
        }

        return ToUserDto(user);
    }

    public async Task ChangePasswordAsync(long userId, string currentPassword, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(userId);
        if (user is null || !user.Active)
        {
            throw new AuthException("El usuario no existe o está desactivado.", StatusCodes.Status401Unauthorized);
        }

        if (!BCrypt.Net.BCrypt.Verify(currentPassword, user.PasswordHash))
        {
            throw new AuthException("La contraseña actual es incorrecta.", StatusCodes.Status400BadRequest);
        }

        await _userRepository.UpdatePasswordAsync(userId, BCrypt.Net.BCrypt.HashPassword(newPassword));
        await _refreshTokenRepository.RevokeAllForUserAsync(userId);
    }

    private async Task<AuthResponse> BuildAuthResponseAsync(
        User user,
        string? ipAddress,
        string? userAgent,
        (string Token, string Hash, DateTime ExpiresAt)? existingRefreshToken = null)
    {
        var permissions = await _permissionRepository.GetByUserIdAsync(user.UserId);
        var (accessToken, accessTokenExpiresAt) = _tokenService.GenerateAccessToken(
            user, permissions?.Profile, permissions?.Permissions);

        var (refreshToken, refreshTokenHash, refreshTokenExpiresAt) = existingRefreshToken ?? _tokenService.GenerateRefreshToken();

        await _refreshTokenRepository.CreateAsync(new RefreshToken
        {
            UserId = user.UserId,
            TokenSha256 = refreshTokenHash,
            ExpiresAt = refreshTokenExpiresAt,
            CreatedAt = DateTime.UtcNow,
            IpAddress = ipAddress,
            UserAgent = userAgent
        });

        return new AuthResponse
        {
            AccessToken = accessToken,
            RefreshToken = refreshToken,
            AccessTokenExpiresAt = accessTokenExpiresAt,
            RefreshTokenExpiresAt = refreshTokenExpiresAt,
            User = ToUserDto(user)
        };
    }

    private static UserDto ToUserDto(User user)
    {
        return new UserDto
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Active = user.Active,
            LastLoginAt = user.LastLoginAt
        };
    }
}