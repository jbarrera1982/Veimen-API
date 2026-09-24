using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Security.Cryptography;
using System.Text;
using Veimen_API.Models;
using Microsoft.IdentityModel.Tokens;

namespace Veimen_API.Services;

public class TokenService
{
    private readonly JwtSettings _settings;

    public TokenService(JwtSettings settings)
    {
        _settings = settings;
    }

    public (string AccessToken, DateTime ExpiresAt) GenerateAccessToken(
        User user, string? profile = null, IReadOnlyCollection<string>? permissions = null)
    {
        var securityKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.Secret));
        var credentials = new SigningCredentials(securityKey, SecurityAlgorithms.HmacSha256);

        var claims = new List<Claim>
        {
            new(JwtRegisteredClaimNames.Sub, user.UserId.ToString()),
            new(JwtRegisteredClaimNames.UniqueName, user.Username),
            new(JwtRegisteredClaimNames.Email, user.Email),
            new(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString()),
            new(JwtRegisteredClaimNames.Iat, DateTimeOffset.UtcNow.ToUnixTimeSeconds().ToString(), ClaimValueTypes.Integer64)
        };

        if (!string.IsNullOrWhiteSpace(user.FullName))
        {
            claims.Add(new(JwtRegisteredClaimNames.Name, user.FullName));
        }

        // Perfil y permisos del usuario: las policies validan los claims 'perm'.
        if (!string.IsNullOrWhiteSpace(profile))
        {
            claims.Add(new(Permissions.ProfileClaimType, profile));
        }

        if (permissions is not null)
        {
            foreach (var permission in permissions)
            {
                claims.Add(new(Permissions.ClaimType, permission));
            }
        }

        var expiresAt = DateTime.UtcNow.AddMinutes(_settings.AccessTokenExpirationMinutes);

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            notBefore: DateTime.UtcNow,
            expires: expiresAt,
            signingCredentials: credentials);

        return (new JwtSecurityTokenHandler().WriteToken(token), expiresAt);
    }

    public (string RefreshToken, string TokenSha256, DateTime ExpiresAt) GenerateRefreshToken()
    {
        var rawToken = Convert.ToBase64String(RandomNumberGenerator.GetBytes(64))
            .Replace("+", "-")
            .Replace("/", "_")
            .TrimEnd('=');

        var expiresAt = DateTime.UtcNow.AddDays(_settings.RefreshTokenExpirationDays);

        return (rawToken, HashToken(rawToken), expiresAt);
    }

    public static string HashToken(string token)
    {
        var sha256 = SHA256.HashData(Encoding.UTF8.GetBytes(token));
        return Convert.ToHexString(sha256).ToLowerInvariant();
    }
}