using Veimen_API.Exceptions;
using Microsoft.AspNetCore.Http;

namespace Veimen_API.Services;

// Idiomas soportados por la aplicación. La base guarda el nombre en español
// ("Español" por defecto) y el token viaja con ese valor en el claim 'language'.
public static class LanguageCatalog
{
    public const string Default = "Español";

    public const string Spanish = "Español";

    public const string English = "Inglés";

    // Valida y normaliza el idioma: vacío o solo espacios usa el idioma por defecto.
    // Acepta el nombre en español o inglés y los códigos ISO es/en.
    public static string Normalize(string? language)
    {
        if (string.IsNullOrWhiteSpace(language))
        {
            return Default;
        }

        var normalized = language.Trim();

        if (normalized.Equals(Spanish, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Espanol", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("es", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Spanish", StringComparison.OrdinalIgnoreCase))
        {
            return Spanish;
        }

        if (normalized.Equals(English, StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("Ingles", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("en", StringComparison.OrdinalIgnoreCase)
            || normalized.Equals("English", StringComparison.OrdinalIgnoreCase))
        {
            return English;
        }

        throw new AuthException("El idioma indicado no es válido.", StatusCodes.Status400BadRequest);
    }
}
