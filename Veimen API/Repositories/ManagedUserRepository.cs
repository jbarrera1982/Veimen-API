using Veimen_API.Data;
using Veimen_API.Models.Dtos;
using Dapper;

namespace Veimen_API.Repositories;

public interface IManagedUserRepository
{
    Task<IReadOnlyList<ManagedUserDto>> ListAsync();
}

public class ManagedUserRepository : IManagedUserRepository
{
    private readonly DapperContext _context;

    public ManagedUserRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ManagedUserDto>> ListAsync()
    {
        const string query = @"
            SELECT u.user_id AS UserId,
                   u.username AS Username,
                   u.email AS Email,
                   u.full_name AS FullName,
                   u.active AS Active,
                   u.profile_id AS ProfileId,
                   p.name AS ProfileName,
                   u.last_login_at AS LastLoginAt,
                   u.created_at AS CreatedAt
            FROM user u
            LEFT JOIN profile p ON p.profile_id = u.profile_id
            ORDER BY u.active DESC, u.username";

        using var connection = _context.CreateConnection();
        var rows = await connection.QueryAsync<ManagedUserDto>(query);
        return rows.ToList();
    }
}
