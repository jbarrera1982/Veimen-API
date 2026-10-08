using Veimen_API.Data;
using Veimen_API.Models;
using Dapper;

namespace Veimen_API.Repositories;

public class ClientRepository : IClientRepository
{
    private readonly DapperContext _context;

    public ClientRepository(DapperContext context)
    {
        _context = context;
    }

    // Alias explícitos para que Dapper mapee snake_case a PascalCase (mismo patrón que
    // PromptRepository). `client` se cita con backticks por seguridad.
    private const string SelectColumns = @"
        client_id AS ClientId,
        name AS Name,
        inbound_email AS InboundEmail,
        outbound_email AS OutboundEmail,
        analyst_email AS AnalystEmail,
        openAI_api_key AS OpenAIApiKey,
        active AS Active";

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        var query = $@"
            SELECT {SelectColumns}
            FROM `client`
            WHERE deleted = 0
            ORDER BY name";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Client>(query);
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        var query = $@"
            SELECT {SelectColumns}
            FROM `client`
            WHERE client_id = @ClientId AND deleted = 0";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Client>(query, new { ClientId = id });
    }

    public async Task<int> CreateAsync(Client client)
    {
        const string insertQuery = @"
            INSERT INTO `client` (
                name, inbound_email, outbound_email, analyst_email, openAI_api_key, active
            ) VALUES (
                @Name, @InboundEmail, @OutboundEmail, @AnalystEmail, @OpenAIApiKey, @Active
            );
            SELECT LAST_INSERT_ID();";
        using var connection = _context.CreateConnection();        
        return await connection.QuerySingleAsync<int>(insertQuery,client);
    }

    public async Task<bool> UpdateAsync(Client client)
    {
        const string query = @"
            UPDATE `client`
            SET name = @Name,
                inbound_email = @InboundEmail,
                outbound_email = @OutboundEmail,
                analyst_email = @AnalystEmail,
                openAI_api_key = @OpenAIApiKey,
                active = @Active
            WHERE client_id = @ClientId AND deleted = 0";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, client);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(int id)
    {
        // Borrado lógico: la fila se marca y deja de devolverse en los GET.
        const string query = @"
            UPDATE `client`
            SET deleted = 1
            WHERE client_id = @ClientId AND deleted = 0";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, new { ClientId = id });
        return affectedRows > 0;
    }
}
