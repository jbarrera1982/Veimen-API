using System.Text;
using Dapper;
using Veimen_API.Data;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public class EmailEvaluationDatasetRepository : IEmailEvaluationDatasetRepository
{
    private readonly DapperContext _context;

    public EmailEvaluationDatasetRepository(DapperContext context)
    {
        _context = context;
    }

    // Alias explícitos para que Dapper mapee snake_case a PascalCase. `from` y `case` son
    // palabras reservadas de MySQL y se citan con backticks.
    private const string DetailColumns = @"
        id AS Id,
        `case` AS `Case`,
        scenario AS Scenario,
        `from` AS `From`,
        subject AS Subject,
        content AS Content,
        client_id AS ClientId,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt,
        created_by AS CreatedBy,
        updated_by AS UpdatedBy";

    private const string ListColumns = @"
        id AS Id,
        `case` AS `Case`,
        scenario AS Scenario,
        `from` AS `From`,
        subject AS Subject,
        LEFT(content, 2000) AS ContentPreview,
        client_id AS ClientId,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt";

    public async Task<(IEnumerable<EmailEvaluationCaseListItem> Items, long TotalCount)> GetPagedAsync(
        int? clientId, string? scenario, string? search, int page, int pageSize)
    {
        var where = new StringBuilder(" WHERE deleted = 0");
        var parameters = new DynamicParameters();

        if (clientId.HasValue)
        {
            where.Append(" AND client_id = @ClientId");
            parameters.Add("ClientId", clientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(scenario))
        {
            where.Append(" AND scenario = @Scenario");
            parameters.Add("Scenario", scenario.Trim());
        }

        if (!string.IsNullOrWhiteSpace(search))
        {
            // Substring literal sobre los campos visibles del caso. Se escapan los
            // comodines de LIKE con '!' como carácter de escape (misma convención
            // que ServiceRequestRepository).
            var pattern = "%" + search.Trim()
                .Replace("!", "!!")
                .Replace("%", "!%")
                .Replace("_", "!_") + "%";

            where.Append(@"
                AND (`case` LIKE @SearchPattern ESCAPE '!'
                  OR scenario LIKE @SearchPattern ESCAPE '!'
                  OR `from` LIKE @SearchPattern ESCAPE '!'
                  OR subject LIKE @SearchPattern ESCAPE '!')");
            parameters.Add("SearchPattern", pattern);
        }

        var itemsQuery = $@"
            SELECT {ListColumns}
            FROM email_evaluation_dataset
            {where}
            ORDER BY created_at DESC, id DESC
            LIMIT @Limit OFFSET @Offset";

        var countQuery = $"SELECT COUNT(*) FROM email_evaluation_dataset{where}";

        parameters.Add("Limit", pageSize);
        parameters.Add("Offset", (page - 1) * pageSize);

        using var connection = _context.CreateConnection();
        var items = await connection.QueryAsync<EmailEvaluationCaseListItem>(itemsQuery, parameters);
        var totalCount = await connection.ExecuteScalarAsync<long>(countQuery, parameters);

        return (items, totalCount);
    }

    public async Task<IEnumerable<string>> GetScenariosAsync(int? clientId)
    {
        const string query = @"
            SELECT DISTINCT scenario
            FROM email_evaluation_dataset
            WHERE deleted = 0
              AND (@ClientId IS NULL OR client_id = @ClientId)
            ORDER BY scenario";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<string>(query, new { ClientId = clientId });
    }

    public async Task<EmailEvaluationDataset?> GetByIdAsync(long id)
    {
        var query = $@"
            SELECT {DetailColumns}
            FROM email_evaluation_dataset
            WHERE id = @Id AND deleted = 0";

        using var connection = _context.CreateConnection();
        return await connection.QueryFirstOrDefaultAsync<EmailEvaluationDataset>(
            query, new { Id = id });
    }

    public async Task<long> CreateAsync(EmailEvaluationDataset entity)
    {
        const string insert = @"
            INSERT INTO email_evaluation_dataset (
                `case`, scenario, `from`, subject, content, client_id, deleted,
                created_at, updated_at, created_by, updated_by
            ) VALUES (
                @Case, @Scenario, @From, @Subject, @Content, @ClientId, 0,
                @CreatedAt, @UpdatedAt, @CreatedBy, @UpdatedBy
            )";

        using var connection = _context.CreateConnection();
        await connection.ExecuteAsync(insert, entity);
        return await connection.ExecuteScalarAsync<long>("SELECT LAST_INSERT_ID()");
    }

    public async Task<bool> UpdateAsync(EmailEvaluationDataset entity)
    {
        const string query = @"
            UPDATE email_evaluation_dataset
            SET `case` = @Case,
                scenario = @Scenario,
                `from` = @From,
                subject = @Subject,
                content = @Content,
                client_id = @ClientId,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND deleted = 0";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(query, entity);
        return affectedRows > 0;
    }

    public async Task<bool> DeleteAsync(long id, DateTime updatedAt, string? updatedBy)
    {
        const string query = @"
            UPDATE email_evaluation_dataset
            SET deleted = 1,
                updated_at = @UpdatedAt,
                updated_by = @UpdatedBy
            WHERE id = @Id AND deleted = 0";

        using var connection = _context.CreateConnection();
        var affectedRows = await connection.ExecuteAsync(
            query, new { Id = id, UpdatedAt = updatedAt, UpdatedBy = updatedBy });
        return affectedRows > 0;
    }
}
