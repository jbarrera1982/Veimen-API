using Veimen_API.Data;
using Veimen_API.Models.Dtos;
using Dapper;

namespace Veimen_API.Repositories;

public class PermissionRepository : IPermissionRepository
{
    private readonly DapperContext _context;

    public PermissionRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<UserPermissionsDto?> GetByUserIdAsync(long userId)
    {
        // Devuelve null si el usuario no tiene perfil asignado o el perfil no tiene permisos.
        const string query = @"
            SELECT
                p.name AS Profile,
                perm.code AS PermissionCode
            FROM user u
            INNER JOIN profile p ON p.profile_id = u.profile_id
            INNER JOIN profile_permission pp ON pp.profile_id = p.profile_id
            INNER JOIN permission perm ON perm.permission_id = pp.permission_id
            WHERE u.user_id = @UserId
            ORDER BY perm.code";

        using var connection = _context.CreateConnection();
        var rows = (await connection.QueryAsync<UserPermissionRow>(query, new { UserId = userId })).ToList();

        if (rows.Count == 0)
        {
            return null;
        }

        return new UserPermissionsDto
        {
            Profile = rows[0].Profile,
            Permissions = rows.Select(r => r.PermissionCode).ToArray()
        };
    }

    // Mapeo de la fila del JOIN (solo uso interno).
    private sealed class UserPermissionRow
    {
        public string? Profile { get; set; }

        public string PermissionCode { get; set; } = string.Empty;
    }
}
