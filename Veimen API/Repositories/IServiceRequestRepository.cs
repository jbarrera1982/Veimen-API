using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public interface IServiceRequestRepository
{
    Task<(IEnumerable<ServiceRequest> Items, long TotalCount)> GetFilteredAsync(
        ServiceRequestQueryParams query, DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestTokenUsageRow>> GetTokenUsageAsync(DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestTraceStep>> GetTraceAsync(long requestNumber);
}
