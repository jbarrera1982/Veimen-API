using Veimen_API.Models;

namespace Veimen_API.Repositories;

public interface IRefreshTokenRepository
{
    Task<long> CreateAsync(RefreshToken token);
    Task<RefreshToken?> GetByTokenHashAsync(string tokenSha256);
    Task<bool> RevokeAsync(long id, string? replacedByToken);
    Task<bool> RevokeAllForUserAsync(long userId);
}