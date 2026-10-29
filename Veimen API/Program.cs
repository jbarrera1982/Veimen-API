
using System.Text;
using Veimen_API.Data;
using Veimen_API.Repositories;
using Veimen_API.Services;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi;

const string FrontendCorsPolicyName = "FrontendPolicy";

var builder = WebApplication.CreateBuilder(args);

var jwtSettings = builder.Configuration
    .GetSection(JwtSettings.SectionName)
    .Get<JwtSettings>() ?? new JwtSettings();

if (string.IsNullOrWhiteSpace(jwtSettings.Secret) ||
    jwtSettings.Secret.Length < 32)
{
    throw new InvalidOperationException(
        "'Jwt:Secret' debe tener al menos 32 caracteres. " +
        "Configúrelo en User Secrets (dev) o en la variable de entorno 'Jwt__Secret' (prod).");
}

if (string.IsNullOrWhiteSpace(jwtSettings.Issuer) ||
    string.IsNullOrWhiteSpace(jwtSettings.Audience))
{
    throw new InvalidOperationException(
        "'Jwt:Issuer' y 'Jwt:Audience' son obligatorios.");
}

// Servicios
builder.Services.AddSingleton(jwtSettings);
builder.Services.AddSingleton<TokenService>();
builder.Services.AddSingleton<DapperContext>();

builder.Services.AddScoped<IPromptRepository, PromptRepository>();
builder.Services.AddScoped<IPromptService, PromptService>();
builder.Services.AddScoped<IClientRepository, ClientRepository>();
builder.Services.AddScoped<IClientService, ClientService>();
builder.Services.AddScoped<IUserRepository, UserRepository>();
builder.Services.AddScoped<IManagedUserRepository, ManagedUserRepository>();
builder.Services.AddScoped<IProfileRepository, ProfileRepository>();
builder.Services.AddScoped<IUserService, UserService>();
builder.Services.AddScoped<IRefreshTokenRepository, RefreshTokenRepository>();
builder.Services.AddScoped<IPermissionRepository, PermissionRepository>();
builder.Services.AddScoped<IAuthService, AuthService>();
builder.Services.AddScoped<IServiceRequestRepository, ServiceRequestRepository>();
builder.Services.AddScoped<IServiceRequestService, ServiceRequestService>();
builder.Services.AddScoped<IMailRepository, MailRepository>();
builder.Services.AddScoped<IMailScanStateRepository, MailScanStateRepository>();
builder.Services.AddScoped<IMailService, MailService>();

builder.Services.AddHttpClient<IAuditService, AuditService>();
builder.Services.AddHttpClient<IUsageService, UsageService>();

builder.Services.AddHealthChecks()
    .AddCheck<DatabaseHealthCheck>("database");

// Autenticación JWT
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(
                Encoding.UTF8.GetBytes(jwtSettings.Secret)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

// Autorización por permisos
builder.Services.AddAuthorization(options =>
{
    foreach (var permission in Permissions.All)
    {
        options.AddPolicy(permission, policy =>
            policy.RequireClaim(Permissions.ClaimType, permission));
    }
});

// CORS
var allowedOrigins = builder.Configuration["Cors:AllowedOrigins"]
    ?? "http://localhost:4200;http://www.veimen.net;https://www.veimen.net;http://veimen.net;https://veimen.net";

var origins = allowedOrigins
    .Split(';', StringSplitOptions.RemoveEmptyEntries |
                StringSplitOptions.TrimEntries)
    .Select(o => o.Trim().TrimEnd('/'))
    .Where(o => o.Length > 0)
    .Distinct(StringComparer.OrdinalIgnoreCase)
    .ToArray();

builder.Services.AddCors(options =>
{
    options.AddPolicy(FrontendCorsPolicyName, policy =>
    {
        policy
            .WithOrigins(origins)
            .AllowAnyHeader()
            .AllowAnyMethod();
    });
});

builder.Services.AddControllers();

// OpenAPI
builder.Services.AddOpenApi(options =>
{
    options.AddDocumentTransformer((document, context, cancellationToken) =>
    {
        document.Components ??= new OpenApiComponents();
        document.Components.SecuritySchemes ??=
            new Dictionary<string, IOpenApiSecurityScheme>();

        document.Components.SecuritySchemes["Bearer"] =
            new OpenApiSecurityScheme
            {
                Type = SecuritySchemeType.Http,
                Scheme = "bearer",
                BearerFormat = "JWT",
                Description = "Token JWT obtenido en POST /api/auth/login"
            };

        document.Security ??= new List<OpenApiSecurityRequirement>();
        document.Security.Add(new OpenApiSecurityRequirement
        {
            {
                new OpenApiSecuritySchemeReference("Bearer", document, null),
                new List<string>()
            }
        });

        return Task.CompletedTask;
    });
});

var app = builder.Build();

// Diagnóstico temporal: registrar método, ruta y estado.
// No registra tokens ni cuerpos de las solicitudes.
app.Use(async (context, next) =>
{
    Console.WriteLine(
        $"HTTP {context.Request.Method} {context.Request.Path}");

    context.Response.OnStarting(() =>
    {
        Console.WriteLine(
            $"HTTP {context.Request.Method}: {context.Response.StatusCode}");

        return Task.CompletedTask;
    });

    await next();
});

// OpenAPI solo en desarrollo
if (app.Environment.IsDevelopment())
{
    app.MapOpenApi();
}

// Pipeline HTTP
app.UseRouting();

// CORS debe ejecutarse antes de autenticación y autorización.
app.UseCors(FrontendCorsPolicyName);

// Conservado para la compatibilidad con el método HTTP override.
app.UseHttpMethodOverride();

app.UseAuthentication();
app.UseAuthorization();

app.MapHealthChecks("/health");
app.MapControllers();

app.Run();