using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public interface IPermissionRepository
{
    Task<UserPermissionsDto?> GetByUserIdAsync(long userId);
}
