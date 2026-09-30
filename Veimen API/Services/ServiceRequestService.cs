using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Repositories;

namespace Veimen_API.Services;

public class ServiceRequestService : IServiceRequestService
{
    private readonly IServiceRequestRepository _repository;

    public ServiceRequestService(IServiceRequestRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<ServiceRequest>> GetFilteredAsync(
        ServiceRequestQueryParams query, DateTime? startDate, DateTime? endDate)
    {
        var (items, totalCount) = await _repository.GetFilteredAsync(query, startDate, endDate);

        return new PagedResult<ServiceRequest>
        {
            Items = items,
            TotalCount = totalCount,
            Page = query.Page,
            PageSize = query.PageSize
        };
    }

    public async Task<IEnumerable<ServiceRequestDashboardRow>> GetDashboardAsync(DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetDashboardAsync(startDate, endDate);
    }

    public async Task<IEnumerable<ServiceRequestTokenUsageRow>> GetTokenUsageAsync(DateTime? startDate, DateTime? endDate)
    {
        return await _repository.GetTokenUsageAsync(startDate, endDate);
    }

    public async Task<IEnumerable<ServiceRequestTraceStep>> GetTraceAsync(long requestNumber)
    {
        return await _repository.GetTraceAsync(requestNumber);
    }
}
