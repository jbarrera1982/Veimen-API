using Veimen_API.Data;
using Veimen_API.Models;
using Dapper;

namespace Veimen_API.Repositories;

public class UserRepository : IUserRepository
{
    private readonly DapperContext _context;

    public UserRepository(DapperContext context)
    {
        _context = context;
    }

    private const string UserColumns = @"
        user_id AS UserId,
        username AS Username,
        email AS Email,
        password_hash AS PasswordHash,
        full_name AS FullName,
        active AS Active,
        profile_id AS ProfileId,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt,
        last_login_at AS LastLoginAt";

    public async Task<User?> GetByIdAsync(long id)
    {
        string query = $@"
            SELECT {UserColumns}
            FROM user
            WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { UserId = id });
    }

    public async Task<User?> GetByIdentifierAsync(string identifier)
    {
        string query = $@"
            SELECT {UserColumns}
            FROM user
            WHERE username = @Identifier OR email = @Identifier
            LIMIT 1";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Identifier = identifier });
    }

    public async Task<User?> GetByEmailAsync(string email)
    {
        string query = $@"
            SELECT {UserColumns}
            FROM user
            WHERE email = @Email";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Email = email });
    }

    public async Task<User?> GetByUsernameAsync(string username)
    {
        string query = $@"
            SELECT {UserColumns}
            FROM user
            WHERE username = @Username";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<User>(query, new { Username = username });
    }

    public async Task<bool> ExistsByEmailAsync(string email)
    {
        const string query = "SELECT COUNT(1) FROM user WHERE email = @Email";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(query, new { Email = email });
    }

    public async Task<bool> ExistsByUsernameAsync(string username)
    {
        const string query = "SELECT COUNT(1) FROM user WHERE username = @Username";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(query, new { Username = username });
    }

    public async Task<bool> ExistsByEmailExcludingIdAsync(string email, long excludeId)
    {
        const string query = "SELECT COUNT(1) FROM user WHERE email = @Email AND user_id <> @UserId";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(query, new { Email = email, UserId = excludeId });
    }

    public async Task<bool> ExistsByUsernameExcludingIdAsync(string username, long excludeId)
    {
        const string query = "SELECT COUNT(1) FROM user WHERE username = @Username AND user_id <> @UserId";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(query, new { Username = username, UserId = excludeId });
    }

    public async Task<long> CreateAsync(User user)
    {
        const string insertQuery = @"
            INSERT INTO user (
                username, email, password_hash, full_name, active, profile_id, created_at, updated_at
            ) VALUES (
                @Username, @Email, @PasswordHash, @FullName, @Active, @ProfileId, @CreatedAt, @UpdatedAt
            )";

        const string selectIdQuery = "SELECT LAST_INSERT_ID()";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(insertQuery, user);
        return await connection.QuerySingleAsync<long>(selectIdQuery);
    }

    public async Task<bool> UpdateLastLoginAsync(long id, DateTime lastLoginAt)
    {
        const string query = @"
            UPDATE user
            SET last_login_at = @LastLoginAt
            WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, new { UserId = id, LastLoginAt = lastLoginAt });
        return affectedRows > 0;
    }

    public async Task<bool> UpdatePasswordAsync(long id, string passwordHash)
    {
        const string query = @"
            UPDATE user
            SET password_hash = @PasswordHash, updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(
            query,
            new { UserId = id, PasswordHash = passwordHash, UpdatedAt = DateTime.UtcNow });
        return affectedRows > 0;
    }

    public async Task<bool> UpdateAsync(User user)
    {
        const string query = @"
            UPDATE user
            SET username = @Username,
                email = @Email,
                full_name = @FullName,
                active = @Active,
                profile_id = @ProfileId,
                updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, new
        {
            UserId = user.UserId,
            Username = user.Username,
            Email = user.Email,
            FullName = user.FullName,
            Active = user.Active,
            ProfileId = user.ProfileId,
            UpdatedAt = DateTime.UtcNow
        });
        return affectedRows > 0;
    }

    public async Task<bool> SetActiveAsync(long id, bool active)
    {
        const string query = @"
            UPDATE user
            SET active = @Active, updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(
            query,
            new { UserId = id, Active = active, UpdatedAt = DateTime.UtcNow });
        return affectedRows > 0;
    }

    public async Task<bool> SetProfileAsync(long id, long? profileId)
    {
        const string query = @"
            UPDATE user
            SET profile_id = @ProfileId, updated_at = @UpdatedAt
            WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(
            query,
            new { UserId = id, ProfileId = profileId, UpdatedAt = DateTime.UtcNow });
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string query = "DELETE FROM user WHERE user_id = @UserId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, new { UserId = id });
        return affectedRows > 0;
    }
}