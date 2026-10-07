using Veimen_API.Data;
using Veimen_API.Models;
using Dapper;

namespace Veimen_API.Repositories;

public class PromptRepository : IPromptRepository
{
    private readonly DapperContext _context;

    public PromptRepository(DapperContext context)
    {
        _context = context;
    }

    // Alias explícitos para que Dapper mapee snake_case a PascalCase (mismo patrón
    // que UserRepository/ServiceRequestRepository): sin alias, prompt_id no llena PromptId.
    private const string SelectColumns = @"
        prompt_id AS PromptId,
        secuence AS Secuence,
        code AS Code,
        name AS Name,
        description AS Description,
        agent_group AS AgentGroup,
        type AS Type,
        llm_model AS LlmModel,
        version AS Version,
        system_prompt AS SystemPrompt,
        user_prompt AS UserPrompt,
        temperature AS Temperature,
        max_tokens AS MaxTokens,
        active AS Active,
        observations AS Observations,
        created_by AS CreatedBy,
        created_at AS CreatedAt,
        updated_by AS UpdatedBy,
        updated_at AS UpdatedAt,
        schema_output AS SchemaOutput";

    public async Task<IEnumerable<Prompt>> GetAllAsync()
    {
        var query = $@"
            SELECT {SelectColumns}
            FROM prompt";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<Prompt>(query);
    }

    public async Task<Prompt?> GetByIdAsync(long id)
    {
        var query = $@"
            SELECT {SelectColumns}
            FROM prompt
            WHERE prompt_id = @PromptId";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<Prompt>(query, new { PromptId = id });
    }

    public async Task<long> CreateAsync(Prompt prompt)
    {
        const string insertQuery = @"
            INSERT INTO prompt (
                secuence, code, name, description, agent_group, type,
                llm_model, version, system_prompt, user_prompt, temperature, max_tokens,
                active, observations, created_by, created_at, updated_by, updated_at, schema_output
            ) VALUES (
                @Secuence, @Code, @Name, @Description, @AgentGroup, @Type,
                @LlmModel, @Version, @SystemPrompt, @UserPrompt, @Temperature, @MaxTokens,
                @Active, @Observations, @CreatedBy, @CreatedAt, @UpdatedBy, @UpdatedAt, @SchemaOutput
            )";

        const string selectIdQuery = "SELECT LAST_INSERT_ID()";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(insertQuery, prompt);
        return await connection.QuerySingleAsync<long>(selectIdQuery);
    }

    public async Task<bool> UpdateAsync(Prompt prompt)
    {
        const string query = @"
            UPDATE prompt
            SET secuence = @Secuence,
                code = @Code,
                name = @Name,
                description = @Description,
                agent_group = @AgentGroup,
                type = @Type,
                llm_model = @LlmModel,
                version = @Version,
                system_prompt = @SystemPrompt,
                user_prompt = @UserPrompt,
                temperature = @Temperature,
                max_tokens = @MaxTokens,
                active = @Active,
                observations = @Observations,
                created_by = @CreatedBy,
                created_at = @CreatedAt,
                updated_by = @UpdatedBy,
                updated_at = @UpdatedAt,
                schema_output = @SchemaOutput
            WHERE prompt_id = @PromptId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, prompt);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(long id)
    {
        const string query = "DELETE FROM prompt WHERE prompt_id = @PromptId";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, new { PromptId = id });
        return affectedRows > 0;
    }
}
