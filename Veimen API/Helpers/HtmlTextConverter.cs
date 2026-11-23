using System.Net;
using System.Text.RegularExpressions;

namespace Veimen_API.Helpers;

// Convierte el HTML del correo a texto plano para el preview del listado y para
// el campo `body` que espera el webhook de n8n.
public static class HtmlTextConverter
{
    private static readonly Regex BlockBreaks = new(
        @"<(br|/p|/div|/li|/tr|/h[1-6])\b[^>]*>",
        RegexOptions.IgnoreCase | RegexOptions.Compiled);

    private static readonly Regex Tags = new(
        "<[^>]+>",
        RegexOptions.Compiled);

    private static readonly Regex Whitespace = new(
        @"\s+",
        RegexOptions.Compiled);

    public static string ToPlainText(string? html)
    {
        if (string.IsNullOrWhiteSpace(html))
        {
            return string.Empty;
        }

        var text = BlockBreaks.Replace(html, " ");
        text = Tags.Replace(text, " ");
        text = WebUtility.HtmlDecode(text);
        return Whitespace.Replace(text, " ").Trim();
    }

    public static string ToPreview(string? html, int maxLength = 200)
    {
        var text = ToPlainText(html);
        return text.Length <= maxLength ? text : text[..maxLength] + "…";
    }
}
