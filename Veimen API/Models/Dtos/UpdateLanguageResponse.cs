namespace Veimen_API.Models.Dtos;

// Respuesta al cambiar el propio idioma. El access token se reemite con el claim
// 'language' nuevo para que el cambio aplique sin volver a iniciar sesión; el
// refresh token se conserva (por eso no se devuelve).
public class UpdateLanguageResponse
{
    public string AccessToken { get; set; } = string.Empty;

    public DateTime AccessTokenExpiresAt { get; set; }

    public UserDto User { get; set; } = new();
}
