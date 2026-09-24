using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public interface IProfileRepository
{
    Task<IReadOnlyList<ProfileDto>> ListAsync();
    Task<bool> ExistsAsync(long profileId);
}
