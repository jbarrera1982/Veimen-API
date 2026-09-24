using Veimen_API.Exceptions;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Repositories;
using Microsoft.AspNetCore.Http;

namespace Veimen_API.Services;

public class UserService : IUserService
{
    private readonly IUserRepository _userRepository;
    private readonly IManagedUserRepository _managedUserRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public UserService(
        IUserRepository userRepository,
        IManagedUserRepository managedUserRepository,
        IProfileRepository profileRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _managedUserRepository = managedUserRepository;
        _profileRepository = profileRepository;
        _refreshTokenRepository = refreshTokenRepository;
    }

    public async Task<IReadOnlyList<ManagedUserDto>> ListAsync()
    {
        return await _managedUserRepository.ListAsync();
    }

    public async Task<IReadOnlyList<ProfileDto>> ListProfilesAsync()
    {
        return await _profileRepository.ListAsync();
    }

    public async Task<ManagedUserDto> CreateAsync(CreateUserRequest request)
    {
        var username = request.Username.Trim();
        var email = request.Email.Trim().ToLowerInvariant();

        if (await _userRepository.ExistsByUsernameAsync(username))
        {
            throw new AuthException("El nombre de usuario ya está en uso.", StatusCodes.Status409Conflict);
        }

        if (await _userRepository.ExistsByEmailAsync(email))
        {
            throw new AuthException("El correo electrónico ya está en uso.", StatusCodes.Status409Conflict);
        }

        if (!await _profileRepository.ExistsAsync(request.ProfileId))
        {
            throw new AuthException("El perfil indicado no existe.", StatusCodes.Status400BadRequest);
        }

        var user = new User
        {
            Username = username,
            Email = email,
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
            Active = true,
            ProfileId = request.ProfileId,
            CreatedAt = DateTime.UtcNow,
            UpdatedAt = DateTime.UtcNow
        };

        var newId = await _userRepository.CreateAsync(user);

        return await GetManagedOrThrowAsync(newId);
    }

    public async Task<ManagedUserDto> UpdateAsync(long id, UpdateUserRequest request, long currentUserId)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new AuthException("El usuario no existe.", StatusCodes.Status404NotFound);
        }

        if (request.ProfileId.HasValue)
        {
            if (!await _profileRepository.ExistsAsync(request.ProfileId.Value))
            {
                throw new AuthException("El perfil indicado no existe.", StatusCodes.Status400BadRequest);
            }

            user.ProfileId = request.ProfileId;
        }

        if (request.Email is not null)
        {
            var email = request.Email.Trim().ToLowerInvariant();
            if (string.IsNullOrWhiteSpace(email))
            {
                throw new AuthException("El correo electrónico no es válido.", StatusCodes.Status400BadRequest);
            }

            if (!string.Equals(email, user.Email, StringComparison.OrdinalIgnoreCase))
            {
                if (await _userRepository.ExistsByEmailExcludingIdAsync(email, id))
                {
                    throw new AuthException("El correo electrónico ya está en uso.", StatusCodes.Status409Conflict);
                }

                user.Email = email;
            }
        }

        if (request.FullName is not null)
        {
            user.FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim();
        }

        if (request.Active.HasValue)
        {
            if (id == currentUserId && !request.Active.Value)
            {
                throw new AuthException("No puedes desactivar tu propio usuario.", StatusCodes.Status400BadRequest);
            }

            user.Active = request.Active.Value;
            if (!user.Active)
            {
                await _refreshTokenRepository.RevokeAllForUserAsync(id);
            }
        }

        await _userRepository.UpdateAsync(user);

        return await GetManagedOrThrowAsync(id);
    }

    public async Task ResetPasswordAsync(long id, string newPassword)
    {
        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new AuthException("El usuario no existe.", StatusCodes.Status404NotFound);
        }

        await _userRepository.UpdatePasswordAsync(id, BCrypt.Net.BCrypt.HashPassword(newPassword));
        await _refreshTokenRepository.RevokeAllForUserAsync(id);
    }

    public async Task DeleteAsync(long id, long currentUserId)
    {
        if (id == currentUserId)
        {
            throw new AuthException("No puedes eliminar tu propio usuario.", StatusCodes.Status400BadRequest);
        }

        var user = await _userRepository.GetByIdAsync(id);
        if (user is null)
        {
            throw new AuthException("El usuario no existe.", StatusCodes.Status404NotFound);
        }

        await _userRepository.DeleteAsync(id);
    }

    private async Task<ManagedUserDto> GetManagedOrThrowAsync(long id)
    {
        var users = await _managedUserRepository.ListAsync();
        var managed = users.FirstOrDefault(u => u.UserId == id);
        if (managed is null)
        {
            throw new AuthException("El usuario no existe.", StatusCodes.Status404NotFound);
        }

        return managed;
    }
}
