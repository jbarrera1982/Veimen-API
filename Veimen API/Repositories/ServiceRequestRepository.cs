using System.Text;
using MySqlConnector;
using Veimen_API.Data;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Dapper;

namespace Veimen_API.Repositories;

public class ServiceRequestRepository : IServiceRequestRepository
{
    // Alias explícitos para que Dapper mapee snake_case a PascalCase.
    // 'from' es palabra reservada en MySQL: requiere backticks.
    private const string SelectColumns = @"
        request_number AS RequestNumber,
        channel AS Channel,
        `from` AS `From`,
        subject AS Subject,
        original_message AS OriginalMessage,
        receipt_date AS ReceiptDate,
        n8n_workflow AS N8nWorkflow,
        status AS Status,
        priority AS Priority,
        created_at AS CreatedAt,
        updated_at AS UpdatedAt,
        description AS Description,
        otp_code AS OtpCode,
        otp_verified_at AS OtpVerifiedAt,
        detected_intent AS DetectedIntent,
        audited AS Audited,
        suspension_type AS SuspensionType";

    private readonly DapperContext _context;

    public ServiceRequestRepository(DapperContext context)
    {
        _context = context;
    }

    public async Task<(IEnumerable<ServiceRequest> Items, long TotalCount)> GetFilteredAsync(
        ServiceRequestQueryParams query, DateTime? startDate, DateTime? endDate)
    {
        var where = new StringBuilder(" WHERE 1 = 1");
        var parameters = new DynamicParameters();

        if (startDate.HasValue)
        {
            where.Append(" AND created_at >= @StartDate");
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Cota exclusiva del día siguiente para incluir el día completo de end_date.
            where.Append(" AND created_at < @EndDateExclusive");
            parameters.Add("EndDateExclusive", endDate.Value.Date.AddDays(1));
        }

        if (!string.IsNullOrWhiteSpace(query.Status))
        {
            where.Append(" AND status = @Status");
            parameters.Add("Status", query.Status);
        }

        if (query.ClientId.HasValue)
        {
            where.Append(" AND client_id = @ClientId");
            parameters.Add("ClientId", query.ClientId.Value);
        }

        if (!string.IsNullOrWhiteSpace(query.Search))
        {
            // Wildcard sobre `from` y `subject` (se escapan %/_/! para tratarlas como substring literal
            // y no como comodín SQL) más coincidencia exacta sobre request_number si es numérico.
            // '!' se usa como carácter de escape de LIKE para evitar problemas de literales con '\'.
            var search = query.Search.Trim();
            var pattern = "%" + search
                .Replace("!", "!!")
                .Replace("%", "!%")
                .Replace("_", "!_") + "%";

            where.Append(" AND (`from` LIKE @SearchPattern ESCAPE '!' OR subject LIKE @SearchPattern ESCAPE '!'");
            parameters.Add("SearchPattern", pattern);

            if (long.TryParse(search, out var requestNumber))
            {
                where.Append(" OR request_number = @RequestNumber");
                parameters.Add("RequestNumber", requestNumber);
            }

            where.Append(")");
        }

        var itemsQuery = $@"
            SELECT {SelectColumns}
            FROM service_request
            {where}
            ORDER BY created_at DESC
            LIMIT @Limit OFFSET @Offset";

        var countQuery = $"SELECT COUNT(*) FROM service_request{where}";

        parameters.Add("Limit", query.PageSize);
        parameters.Add("Offset", (query.Page - 1) * query.PageSize);

        using var connection = _context.CreateConnection();
        var items = await connection.QueryAsync<ServiceRequest>(itemsQuery, parameters);
        var totalCount = await connection.ExecuteScalarAsync<long>(countQuery, parameters);

        return (items, totalCount);
    }

    public async Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(
        DateTime? startDate, DateTime? endDate, int? clientId)
    {
        // El filtro va sobre receipt_date directo (no DATE(receipt_date)) para no perder el uso de índices.
        var where = new StringBuilder(" WHERE 1 = 1");
        var parameters = new DynamicParameters();

        if (startDate.HasValue)
        {
            where.Append(" AND receipt_date >= @StartDate");
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Cota exclusiva del día siguiente para incluir el día completo de end_date.
            where.Append(" AND receipt_date < @EndDateExclusive");
            parameters.Add("EndDateExclusive", endDate.Value.Date.AddDays(1));
        }

        if (clientId.HasValue)
        {
            where.Append(" AND client_id = @ClientId");
            parameters.Add("ClientId", clientId.Value);
        }

        var query = $@"
            SELECT DATE(receipt_date) AS Date,
                   status AS Status,
                   COUNT(*) AS Total
            FROM service_request
            {where}
            GROUP BY DATE(receipt_date), status
            ORDER BY DATE(receipt_date)";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ServiceRequestDashboardRow>(query, parameters);
    }

    public async Task<IEnumerable<ServiceRequestTokenUsageRow>> GetTokenUsageAsync(
        DateTime? startDate, DateTime? endDate, int? clientId)
    {
        // Consumo de tokens por día (end_date de la traza), node y llm_model. Solo nodos LLM.
        // El filtro de fechas va sobre end_date directo (no DATE(end_date)) para no perder el índice;
        // el DATE() se aplica recién en el GROUP BY para obtener el día.
        var where = new StringBuilder(" WHERE node_type = 'LLM'");
        var parameters = new DynamicParameters();

        if (startDate.HasValue)
        {
            where.Append(" AND end_date >= @StartDate");
            parameters.Add("StartDate", startDate.Value);
        }

        if (endDate.HasValue)
        {
            // Cota exclusiva del día siguiente para incluir el día completo de end_date.
            where.Append(" AND end_date < @EndDateExclusive");
            parameters.Add("EndDateExclusive", endDate.Value.Date.AddDays(1));
        }

        if (clientId.HasValue)
        {
            // service_request_trace no tiene client_id: se acota por el requerimiento dueño
            // del paso (service_request.client_id).
            where.Append(
                " AND request_number IN (SELECT request_number FROM service_request WHERE client_id = @ClientId)");
            parameters.Add("ClientId", clientId.Value);
        }

        // SUM() sobre columnas int devuelve DECIMAL en MySQL; el CAST a SIGNED hace que
        // el resultado sea BIGINT y Dapper lo mapee exacto a long sin conversiones implícitas.
        var query = $@"
            SELECT
                DATE(end_date) AS Date,
                node AS Node,
                llm_model AS LlmModel,
                CAST(COALESCE(SUM(input_tokens), 0) AS SIGNED) AS InputTokens,
                CAST(COALESCE(SUM(output_tokens), 0) AS SIGNED) AS OutputTokens,
                CAST(COALESCE(SUM(total_tokens), 0) AS SIGNED) AS TotalTokens
            FROM service_request_trace
            {where}
            GROUP BY DATE(end_date), node, llm_model
            ORDER BY DATE(end_date), node, llm_model";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ServiceRequestTokenUsageRow>(query, parameters);
    }

    public async Task<IEnumerable<ServiceRequestTraceStep>> GetTraceAsync(long requestNumber)
    {
        // Alias explícitos para que Dapper mapee snake_case a PascalCase.
        // 'sequence' es palabra reservada en MySQL: requiere backticks.
        const string query = @"
            SELECT
                trace_id AS TraceId,
                request_number AS RequestNumber,
                `sequence` AS Sequence,
                node AS Node,
                node_type AS NodeType,
                llm_model AS LlmModel,
                prompt_version AS PromptVersion,
                start_date AS StartDate,
                end_date AS EndDate,
                duration_ms AS DurationMs,
                status AS Status,
                confidence AS Confidence,
                input_json AS InputJson,
                output_json AS OutputJson,
                observations AS Observations,
                created_at AS CreatedAt,
                prompt_id AS PromptId,
                prompt_result AS PromptResult
            FROM service_request_trace
            WHERE request_number = @RequestNumber
            ORDER BY trace_id";

        using var connection = _context.CreateConnection();
        return await connection.QueryAsync<ServiceRequestTraceStep>(query, new { RequestNumber = requestNumber });
    }

    public async Task<ReviewOutcome> ReviewAsync(
        long requestNumber,
        string newStatus,
        string observations,
        string reviewerName)
    {
        // CreateConnection() devuelve IDbConnection; los métodos async (OpenAsync,
        // BeginTransactionAsync) son de MySqlConnection, el tipo concreto que crea el context.
        using var connection = (MySqlConnection)_context.CreateConnection();
        await connection.OpenAsync();

        // UPDATE + INSERT en la misma transacción: o se registra todo la revisión o nada.
        using var transaction = await connection.BeginTransactionAsync();

        // Bloquea la fila del requerimiento para serializar revisiones concurrentes
        // sobre el mismo request_number (dos revisores simultáneos no podrían pasar
        // ambos la guarda de 'Pending review').
        const string statusQuery = @"
            SELECT status AS Status
            FROM service_request
            WHERE request_number = @RequestNumber
            FOR UPDATE";

        var currentStatus = await connection.QueryFirstOrDefaultAsync<string?>(
            statusQuery, new { RequestNumber = requestNumber }, transaction);

        if (currentStatus is null)
        {
            await transaction.RollbackAsync();
            return ReviewOutcome.NotFound;
        }

        if (!string.Equals(currentStatus, "Pending review", StringComparison.Ordinal))
        {
            await transaction.RollbackAsync();
            return ReviewOutcome.NotPending;
        }

        const string updateStatusQuery = @"
            UPDATE service_request
            SET status = @NewStatus,
                updated_at = UTC_TIMESTAMP(),
                description = @Description
            WHERE request_number = @RequestNumber";

        var reviewLabel = string.Equals(newStatus, "Closed", StringComparison.Ordinal)
            ? "Cerrado"
            : "Rechazado";
        var description = $"{reviewLabel} por {reviewerName}";

        await connection.ExecuteAsync(
            updateStatusQuery,
            new
            {
                NewStatus = newStatus,
                Description = description,
                RequestNumber = requestNumber,
            },
            transaction);

        // node_type 'MANUAL' (no 'LLM') para que el paso no sume en el dashboard de tokens,
        // que filtra por node_type = 'LLM'. sequence continúa la numeración lógica del flujo.
        const string insertTraceQuery = @"
            INSERT INTO service_request_trace (
                request_number, node, node_type, status, observations,
                start_date, end_date, created_at
            ) VALUES (
                @RequestNumber,
                'Revisión manual', 'MANUAL', 'completed', @Observations,
                UTC_TIMESTAMP(), UTC_TIMESTAMP(), UTC_TIMESTAMP()
            )";

        await connection.ExecuteAsync(
            insertTraceQuery,
            new { RequestNumber = requestNumber, Observations = observations },
            transaction);

        await transaction.CommitAsync();
        return ReviewOutcome.Success;
    }
}
