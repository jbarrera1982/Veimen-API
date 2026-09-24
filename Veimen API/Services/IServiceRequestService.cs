using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IServiceRequestService
{
    Task<PagedResult<ServiceRequest>> GetFilteredAsync(
        ServiceRequestQueryParams query, DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(DateTime? startDate, DateTime? endDate);

    Task<IEnumerable<ServiceRequestTraceStep>> GetTraceAsync(long requestNumber);
}
