using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Services;

public interface IEmailEvaluationDatasetService
{
    Task<PagedResult<EmailEvaluationCaseListItem>> GetPagedAsync(
        int? clientId, string? scenario, string? search, int page, int pageSize);

    Task<IEnumerable<string>> GetScenariosAsync(int? clientId);

    Task<EmailEvaluationDataset?> GetByIdAsync(long id);

    // Devuelve el id autonumérico asignado al caso.
    Task<long> CreateAsync(EmailEvaluationDataset entity, string? userEmail);

    Task<bool> UpdateAsync(EmailEvaluationDataset entity, string? userEmail);

    Task<bool> DeleteAsync(long id, string? userEmail);
}
