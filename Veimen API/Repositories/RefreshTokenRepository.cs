using Veimen_API.Data;
using Veimen_API.Models;
using Dapper;

namespace Veimen_API.Repositories;

public class RefreshTokenRepository : IRefreshTokenRepository
{
    private readonly DapperContext _context;

    public RefreshTokenRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<long> CreateAsync(RefreshToken token)
    {
        const string insertQuery = @"
            INSERT INTO refresh_token (
                user_id, token_sha256, expires_at, created_at, ip_address, user_agent
            ) VALUES (
                @UserId, @TokenSha256, @ExpiresAt, @CreatedAt, @IpAddress, @UserAgent
            )";

        const string selectIdQuery = "SELECT LAST_INSERT_ID()";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(insertQuery, token);
        return await connection.QuerySingleAsync<long>(selectIdQuery);
    }

    public async Task<RefreshToken?> GetByTokenHashAsync(string tokenSha256)
    {
        const string query = @"
            SELECT refresh_token_id AS RefreshTokenId,
                   user_id AS UserId,
                   token_sha256 AS TokenSha256,
                   expires_at AS ExpiresAt,
                   created_at AS CreatedAt,
                   revoked_at AS RevokedAt,
                   replaced_by_token AS ReplacedByToken,
                   ip_address AS IpAddress,
                   user_agent AS UserAgent
            FROM refresh_token
            WHERE token_sha256 = @TokenSha256";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<RefreshToken>(query, new { TokenSha256 = tokenSha256 });
    }

    public async Task<bool> RevokeAsync(long id, string? replacedByToken)
    {
        const string query = @"
            UPDATE refresh_token
            SET revoked_at = @RevokedAt, replaced_by_token = @ReplacedByToken
            WHERE refresh_token_id = @RefreshTokenId AND revoked_at IS NULL";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(
            query,
            new
            {
                RefreshTokenId = id,
                RevokedAt = DateTime.UtcNow,
                ReplacedByToken = replacedByToken
            });
        return affectedRows > 0;
    }

    public async Task<bool> RevokeAllForUserAsync(long userId)
    {
        const string query = @"
            UPDATE refresh_token
            SET revoked_at = @RevokedAt
            WHERE user_id = @UserId AND revoked_at IS NULL";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(
            query,
            new { UserId = userId, RevokedAt = DateTime.UtcNow });
        return affectedRows > 0;
    }
}