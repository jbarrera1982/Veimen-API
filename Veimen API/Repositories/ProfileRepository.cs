using Veimen_API.Data;
using Veimen_API.Models.Dtos;
using Dapper;

namespace Veimen_API.Repositories;

public class ProfileRepository : IProfileRepository
{
    private readonly DapperContext _context;

    public ProfileRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<IReadOnlyList<ProfileDto>> ListAsync()
    {
        const string query = @"
            SELECT profile_id AS ProfileId,
                   name AS Name,
                   description AS Description
            FROM profile
            ORDER BY name";

        using var connection = _context.CreateConnection();
        var rows = await connection.QueryAsync<ProfileDto>(query);
        return rows.ToList();
    }

    public async Task<bool> ExistsAsync(long profileId)
    {
        const string query = "SELECT COUNT(1) FROM profile WHERE profile_id = @ProfileId";

        using var connection = _context.CreateConnection();
        return await connection.ExecuteScalarAsync<bool>(query, new { ProfileId = profileId });
    }
}
