namespace Veimen_API.Services;

// Códigos de permiso conocidos. Deben coincidir con los registros de la tabla 'permission'
// (script 002_create_profiles_tables.sql). Program.cs registra una policy por código
// y TokenService los emite como claims 'perm' en el access token.
public static class Permissions
{
    // Tipo de claim del JWT que transporta un código de permiso.
    public const string ClaimType = "perm";

    // Tipo de claim del JWT con el nombre del perfil del usuario.
    public const string ProfileClaimType = "profile";

    public const string PromptsRead = "prompts.read";
    public const string PromptsWrite = "prompts.write";
    public const string ServiceRequestsRead = "service-requests.read";
    public const string DashboardRead = "dashboard.read";
    public const string TokensRead = "tokens.read";
    public const string UsersManage = "users.manage";
    public const string UsageRead = "usage.read";

    public static readonly string[] All =
        [PromptsRead, PromptsWrite, ServiceRequestsRead, DashboardRead, TokensRead, UsersManage, UsageRead];
}
