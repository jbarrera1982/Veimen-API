using Veimen_API.Models;

namespace Veimen_API.Repositories;

public interface IUserRepository
{
    Task<User?> GetByIdAsync(long id);
    Task<User?> GetByIdentifierAsync(string identifier);
    Task<User?> GetByEmailAsync(string email);
    Task<User?> GetByUsernameAsync(string username);
    Task<bool> ExistsByEmailAsync(string email);
    Task<bool> ExistsByUsernameAsync(string username);
    Task<bool> ExistsByEmailExcludingIdAsync(string email, long excludeId);
    Task<bool> ExistsByUsernameExcludingIdAsync(string username, long excludeId);
    Task<long> CreateAsync(User user);
    Task<bool> UpdateLastLoginAsync(long id, DateTime lastLoginAt);
    Task<bool> UpdatePasswordAsync(long id, string passwordHash);
    Task<bool> UpdateAsync(User user);
    Task<bool> SetActiveAsync(long id, bool active);
    Task<bool> SetProfileAsync(long id, long? profileId);
}