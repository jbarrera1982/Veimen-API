using Veimen_API.Exceptions;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Repositories;
using Microsoft.AspNetCore.Http;
using System.Text.RegularExpressions;

namespace Veimen_API.Services;

public class UserService : IUserService
{
    // Teléfono opcional: admite prefijo +, dígitos, espacios, guiones, puntos y paréntesis.
    private static readonly Regex PhonePattern = new(@"^\+?[0-9\s\-().]{6,30}$", RegexOptions.Compiled);

    // Idioma por defecto (coincide con el default de la columna user.language).
    private const string DefaultLanguage = "Español";

    // Idiomas soportados; se guardan en la base con su nombre en español.
    private const string SpanishLanguage = "Español";
    private const string EnglishLanguage = "Inglés";

    private readonly IUserRepository _userRepository;
    private readonly IManagedUserRepository _managedUserRepository;
    private readonly IProfileRepository _profileRepository;
    private readonly IClientRepository _clientRepository;
    private readonly IRefreshTokenRepository _refreshTokenRepository;

    public UserService(
        IUserRepository userRepository,
        IManagedUserRepository managedUserRepository,
        IProfileRepository profileRepository,
        IClientRepository clientRepository,
        IRefreshTokenRepository refreshTokenRepository)
    {
        _userRepository = userRepository;
        _managedUserRepository = managedUserRepository;
        _profileRepository = profileRepository;
        _clientRepository = clientRepository;
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

        var clientId = await ResolveClientAsync(request.ClientId);

        var user = new User
        {
            Username = username,
            Email = email,
            Phone = NormalizePhone(request.Phone),
            PasswordHash = BCrypt.Net.BCrypt.HashPassword(request.Password),
            FullName = string.IsNullOrWhiteSpace(request.FullName) ? null : request.FullName.Trim(),
            Active = true,
            ProfileId = request.ProfileId,
            ClientId = clientId,
            Language = NormalizeLanguage(request.Language),
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

        if (request.ClientId.HasValue)
        {
            // Semántica PATCH: <= 0 quita el cliente; > 0 asigna (y valida que exista).
            user.ClientId = request.ClientId.Value <= 0
                ? null
                : await ResolveClientAsync(request.ClientId);
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

        if (request.Phone is not null)
        {
            user.Phone = NormalizePhone(request.Phone);
        }

        if (request.Language is not null)
        {
            user.Language = NormalizeLanguage(request.Language);
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

    // Valida y normaliza el teléfono: vacío o solo espacios significa "sin teléfono".
    private static string? NormalizePhone(string? phone)
    {
        if (string.IsNullOrWhiteSpace(phone))
        {
            return null;
        }

        var normalized = phone.Trim();
        if (!PhonePattern.IsMatch(normalized))
        {
            throw new AuthException("El teléfono no es válido.", StatusCodes.Status400BadRequest);
        }

        return normalized;
    }

    // Valida y normaliza el idioma: vacío o solo espacios usa el idioma por defecto
    // ("Español"). Acepta el nombre en español o inglés y los códigos ISO es/en.
    private static string NormalizeLanguage(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return DefaultLanguage;
        }

        var normalized = language.Trim();

        if (normalized.Equals(SpanishLanguage, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Espanol", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("es", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Spanish", StringComparison.OrdinalIgnoreCase))
        {
            return SpanishLanguage;
        }

        if (normalized.Equals(EnglishLanguage, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Ingles", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("en", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("English", StringComparison.OrdinalIgnoreCase))
        {
            return EnglishLanguage;
        }

        throw new AuthException("El idioma indicado no es válido.", StatusCodes.Status400BadRequest);
    }

    // Valida y normaliza el cliente asociado: null o <= 0 significa "sin cliente".
    private async Task<int?> ResolveClientAsync(int? clientId)
    {
        if (clientId is null || clientId.Value <= 0)
        {
            return null;
        }

        var client = await _clientRepository.GetByIdAsync(clientId.Value);
        if (client is null)
        {
            throw new AuthException("El cliente indicado no existe.", StatusCodes.Status400BadRequest);
        }

        return client.ClientId;
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
