using System.Text;
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

    public async Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(DateTime? startDate, DateTime? endDate)
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
                agent AS Agent,
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
}
