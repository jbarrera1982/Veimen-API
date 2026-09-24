using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IUserService
{
    Task<IReadOnlyList<ManagedUserDto>> ListAsync();
    Task<IReadOnlyList<ProfileDto>> ListProfilesAsync();
    Task<ManagedUserDto> CreateAsync(CreateUserRequest request);
    Task<ManagedUserDto> UpdateAsync(long id, UpdateUserRequest request, long currentUserId);
    Task ResetPasswordAsync(long id, string newPassword);
}
