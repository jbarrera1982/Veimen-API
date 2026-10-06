using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IUsageService
{
    Task<OpenAiPage<OpenAiCostResult>> GetCostsAsync(DateTime startDate, DateTime? endDate);

    Task<OpenAiPage<OpenAiCompletionsUsageResult>> GetCompletionsUsageAsync(DateTime startDate, DateTime? endDate);
}
