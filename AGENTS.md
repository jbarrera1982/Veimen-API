# AGENTS.md — Veimen API

API REST en ASP.NET Core para Veimen. Gestiona prompts de LLM y autenticación de usuarios con JWT (access + refresh tokens).

## Stack

- **.NET 10** (`net10.0`), ASP.NET Core Web API, C# con `Nullable` e `ImplicitUsings` habilitados
- **MySQL** vía [MySqlConnector](https://mysqlconnector.net/) + **Dapper** (SQL crudo, sin EF Core ni migraciones)
- **Autenticación**: JWT Bearer (`Microsoft.AspNetCore.Authentication.JwtBearer`), contraseñas con **BCrypt.Net-Next**
- **OpenAPI**: `Microsoft.AspNetCore.OpenApi` (solo expuesto en Development)
- **Despliegue**: Docker (multi-stage `Dockerfile`) + `docker-compose.yml` + `render.yaml`

## Estructura

```
Veimen API/            → raíz del repo (solución .slnx, Dockerfile, docker-compose.yml)
└── Veimen API/        → proyecto web
    ├── Controllers/         → AuthController, PromptsController, ServiceRequestsController
    ├── Services/            → lógica de negocio (interfaces I*Service + implementaciones)
    ├── Repositories/        → acceso a datos (interfaces I*Repository + implementaciones Dapper)
    ├── Data/                → DapperContext (factory de conexiones MySQL)
    ├── Models/              → entidades (Prompt, User, RefreshToken, ServiceRequest) y Dtos/
    ├── Exceptions/          → excepciones de dominio
    ├── Helpers/             → utilidades compartidas (DateQueryParser: fechas de query en yyyyMMdd)
    ├── Scripts/             → SQL de creación de tablas (000_*, 001_*), se ejecutan manualmente
    └── Program.cs           → DI, JWT, CORS, health check en /health
```

El namespace raíz es `Veimen_API` (el nombre de carpeta/proyecto lleva espacio, el namespace no).

## Comandos

```powershell
# Restaurar y compilar
dotnet build

# Ejecutar en desarrollo (https://localhost:7xxx, OpenAPI en /openapi/v1.json)
dotnet run --project "Veimen API"

# Publicar
dotnet publish "Veimen API/Veimen API.csproj" -c Release

# Docker (lee variables desde .env en la raíz)
docker-compose up --build   # expone http://localhost:8080, health check en /health
```

No hay proyecto de tests actualmente.

## Configuración y secretos

**Nunca** escribas credenciales reales en `appsettings.json` ni las subas al repo.

- **Development**: usa [User Secrets](https://learn.microsoft.com/aspnet/core/security/app-secrets) (el `.csproj` ya tiene `UserSecretsId`):
  ```powershell
  dotnet user-secrets set "ConnectionStrings:DefaultConnection" "Server=...;Port=3306;Database=...;Uid=...;Pwd=...;" --project "Veimen API"
  dotnet user-secrets set "Jwt:Secret" "clave-aleatoria-de-al-menos-32-caracteres" --project "Veimen API"
  ```
- **Producción / Docker**: variables de entorno con doble guion bajo (`ConnectionStrings__DefaultConnection`, `Jwt__Secret`). Se inyectan desde `.env` (copiar `.env.example` → `.env` y completar).

Reglas validadas al arrancar (`Program.cs`): `Jwt:Secret` es obligatorio y debe tener **≥ 32 caracteres**; `Jwt:Issuer` y `Jwt:Audience` son obligatorios.

## Convenciones

- **Arquitectura en capas**: `Controller` → `Service` (lógica) → `Repository` (SQL). Los controladores no contienen SQL ni lógica de negocio.
- Toda dependencia se registra en `Program.cs` con inyección de dependencias: `Singleton` para `TokenService`/`DapperContext`, `Scoped` para servicios y repositorios.
- Cada servicio/repositorio tiene su interfaz (`IPromptService`, `IUserRepository`, ...) en el mismo directorio.
- **Acceso a datos**: Dapper con consultas SQL escritas a mano, **siempre parametrizadas** (`@Param` con objeto anónimo o la entidad). Las tablas MySQL usan `snake_case`; los modelos C# usan `PascalCase`.
- Async/await en toda la capa de datos (`QueryAsync`, `ExecuteAsync`, ...). Conexiones con `using var connection = _context.CreateConnection();`.
- Cambios de esquema: crear un nuevo script numerado en `Scripts/` (`00X_descripcion.sql`) y aplicarlo manualmente a la base de datos; no hay migraciones automáticas.
- Los mensajes de error y comentarios del codebase están en **español**.
- **Fechas en query string**: se reciben como `string` en formato **`yyyyMMdd`** (ej: `20260131`) y se parsean con `Helpers/DateQueryParser.TryParse` (parámetro ausente → `null`, sin filtro; formato inválido → responder 400). El `end_date` es **inclusivo**: el repositorio filtra con cota exclusiva del día siguiente (`end_date.Date.AddDays(1)`). Aplica a todo filtro de fecha de endpoints actuales y futuros.
- **`Veimen API.http` siempre actualizado**: al crear o modificar métodos en cualquier controlador, actualiza el archivo `.http` **en el mismo cambio, sin que el usuario lo pida**: agrega la petición de ejemplo para endpoints nuevos (con todos sus parámetros de ruta, query y cuerpo) y ajusta las peticiones existentes si cambian ruta, parámetros o cuerpo.

## Endpoints principales

- `POST /api/auth/login` → devuelve access token JWT (+ refresh token)
- `POST /api/auth/refresh` → renueva el access token
- `GET /api/auth/me/permissions` → devuelve `{ profile, permissions }` leídos de los claims del access token del usuario autenticado (los mismos que aplican las policies). El frontend lo consulta al iniciar la app para habilitar/deshabilitar acciones (solo UX; la seguridad real la aplican las policies del backend).
- `/api/prompts/*` → CRUD de prompts (requiere Bearer + policy `prompts.read` para GET / `prompts.write` para POST-PUT-DELETE)
- `GET /api/ServiceRequests` → lista paginada de service requests (requiere Bearer + policy `service-requests.read`). Filtros opcionales por query string: `start_date`, `end_date` (formato `yyyyMMdd`, rango inclusivo sobre `created_at`), `status` (valor único), `search` (coincidencia exacta sobre `request_number` si el valor es numérico y wildcard/substring sobre `from` y `subject`; los comodines SQL `%`/`_`/`!` del input se escapan, con `!` como carácter de escape de LIKE), `page`/`pageSize`. Devuelve `PagedResult<ServiceRequest>` (items, totalCount, page, pageSize).
- `GET /api/ServiceRequests/dashboard` → totales agrupados por día (`receipt_date`) y `status` (requiere Bearer + policy `dashboard.read`). Filtros opcionales: `start_date`, `end_date` (formato `yyyyMMdd`, rango inclusivo sobre `receipt_date`); sin fechas devuelve todo el histórico. Devuelve lista plana `{ date, status, total }` ordenada cronológicamente.
- `GET /api/ServiceRequests/trace` → traza de un requerimiento (requiere Bearer + policy `service-requests.read`). Parámetro obligatorio `request_number` (entero > 0); si es ≤ 0 responde 400. Devuelve lista plana de `ServiceRequestTraceStep` ordenada por `trace_id`. Las columnas JSON (`input_json`, `output_json`) y `confidence` se devuelven como string, por convención del proyecto.
- `GET /api/ServiceRequests/tokens` → consumo de tokens por día, `node` y `llm_model` (requiere Bearer + policy `tokens.read`). Solo pasos con `node_type = 'LLM'`. Filtros opcionales: `start_date`, `end_date` (formato `yyyyMMdd`, rango inclusivo aplicado sobre `end_date` de la traza; sin fechas devuelve todo el histórico). Devuelve lista plana de `ServiceRequestTokenUsageRow` (`date`, `node`, `llmModel`, `inputTokens`, `outputTokens`, `totalTokens`) ordenada cronológicamente. `llmModel` puede ser `null` si la traza no registró modelo.
- `GET /health` → health check (incluye check de base de datos)
- OpenAPI: `/openapi/v1.json` (solo Development), con esquema de seguridad Bearer documentado

## Autorización (perfiles y permisos)

RBAC simple, administrado directamente en BD (no hay CRUD de perfiles): `user.profile_id` → `profile` → `profile_permission` → `permission` (script `002_create_profiles_tables.sql`; los seeds asignan todos los usuarios existentes al perfil Admin).

- Los códigos de permiso viven en `Services/Permissions.cs` (`prompts.read`, `prompts.write`, `service-requests.read`, `dashboard.read`, `tokens.read`, `users.manage`). Los registros de la tabla `permission` deben coincidir con esas constantes.
- `Program.cs` registra una policy por código: `RequireClaim(Permissions.ClaimType /* "perm" */, code)`.
- `AuthService` carga los permisos del usuario en login/refresh (vía `IPermissionRepository`) y `TokenService` los emite como claims `perm` (+ claim `profile`) en el access token. Como el token vive 15 min, los cambios de permisos en BD aplican al siguiente refresh.
- Los endpoints se protegen con `[Authorize(Policy = Permissions.Xxx)]` además del `[Authorize]` de clase. Un usuario sin perfil no obtiene claims y recibe 403.
- Nuevo permiso: agregar la constante en `Permissions.cs`, el registro en la tabla `permission` (script SQL) y el `[Authorize(Policy = ...)]` donde aplique.

## Notas

- CORS configurado en `Program.cs` vía `Cors:AllowedOrigins` (lista separada por `;`). Por defecto: `http://localhost:4200` (frontend Angular) y la URL de Render.
- La solución usa el formato `.slnx` (XML), no `.sln`.
