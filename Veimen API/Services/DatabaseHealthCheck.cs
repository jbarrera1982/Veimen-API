using Veimen_API.Data;
using Dapper;
using Microsoft.Extensions.Diagnostics.HealthChecks;

namespace Veimen_API.Services;

public class DatabaseHealthCheck : IHealthCheck
{
    private readonly DapperContext _context;

    public DatabaseHealthCheck(DapperContext context)
    {
        _context = context;
    }

    public async Task<HealthCheckResult> CheckHealthAsync(
        HealthCheckContext context,
        CancellationToken cancellationToken = default)
    {
        try
        {
            using var connection = _context.CreateConnection();
            var result = await connection.ExecuteScalarAsync<int>("SELECT 1");
            return result == 1
                ? HealthCheckResult.Healthy("Conexión a la base de datos OK.")
                : HealthCheckResult.Unhealthy("Respuesta inesperada de la base de datos.");
        }
        catch (Exception ex)
        {
            return HealthCheckResult.Unhealthy("No se pudo conectar a la base de datos.", ex);
        }
    }
}