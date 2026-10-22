using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IServiceRequestService
{
    Task<PagedResult<ServiceRequest>> GetFilteredAsync(
        ServiceRequestQueryParams query, DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(
        DateTime? startDate, DateTime? endDate, int? clientId);

    Task<IEnumerable<ServiceRequestTokenUsageRow>> GetTokenUsageAsync(
        DateTime? startDate, DateTime? endDate, int? clientId);

    Task<IEnumerable<ServiceRequestTraceStep>> GetTraceAsync(long requestNumber);

    // Registra una revisión manual: cambia el estado del requerimiento (solo si está en
    // 'Pending review') y crea el paso de traza correspondiente, en una única transacción.
    Task<ReviewOutcome> ReviewAsync(
        long requestNumber,
        string newStatus,
        string observations,
        string reviewerName);
}
