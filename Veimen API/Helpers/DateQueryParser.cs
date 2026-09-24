using System.Globalization;

namespace Veimen_API.Helpers;

// Parseo de fechas de query string en formato yyyyMMdd (convención del proyecto, ver AGENTS.md).
public static class DateQueryParser
{
    public const string Format = "yyyyMMdd";

    // Devuelve true con date = null si el parámetro está ausente (sin filtro);
    // false si llegó un valor pero no cumple el formato.
    public static bool TryParse(string? value, out DateTime? date)
    {
        date = null;

        if (string.IsNullOrWhiteSpace(value))
        {
            return true;
        }

        if (DateTime.TryParseExact(value, Format, CultureInfo.InvariantCulture, DateTimeStyles.None, out var parsed))
        {
            date = parsed.Date;
            return true;
        }

        return false;
    }
}
