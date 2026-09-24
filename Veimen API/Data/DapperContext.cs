using System.Data;
using MySqlConnector;

namespace Veimen_API.Data;

public class DapperContext
{
    private const string DefaultConnectionName = "DefaultConnection";
    private const string FallbackEnvironmentVariable = "MYSQL_CONNECTION_STRING";

    private readonly string _connectionString;

    public DapperContext(IConfiguration configuration)
    {
        _connectionString = configuration.GetConnectionString(DefaultConnectionName)
            ?? configuration[FallbackEnvironmentVariable]
            ?? throw new InvalidOperationException(
                $"No se encontró la cadena de conexión. " +
                $"Configure 'ConnectionStrings:{DefaultConnectionName}' (User Secrets, appsettings o variable de entorno con '__') " +
                $"o la variable de entorno '{FallbackEnvironmentVariable}'.");
    }

    public IDbConnection CreateConnection() => new MySqlConnection(_connectionString);
}
