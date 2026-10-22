using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public interface IServiceRequestRepository
{
    Task<(IEnumerable<ServiceRequest> Items, long TotalCount)> GetFilteredAsync(
        ServiceRequestQueryParams query, DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(
        DateTime? startDate, DateTime? endDate, int? clientId);

    Task<IEnumerable<ServiceRequestTokenUsageRow>> GetTokenUsageAsync(
        DateTime? startDate, DateTime? endDate, int? clientId);

    Task<IEnumerable<ServiceRequestTraceStep>> GetTraceAsync(long requestNumber);

    // Revisión manual: en una transacción cambia el estado del requerimiento (solo si está
    // en 'Pending review') e inserta el paso 'Revisión manual' en service_request_trace.
    Task<ReviewOutcome> ReviewAsync(
        long requestNumber,
        string newStatus,
        string observations,
        string reviewerName);
}
