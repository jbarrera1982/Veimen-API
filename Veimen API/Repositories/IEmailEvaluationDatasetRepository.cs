using Veimen_API.Models;
using Veimen_API.Models.Dtos;

namespace Veimen_API.Repositories;

public interface IEmailEvaluationDatasetRepository
{
    Task<(IEnumerable<EmailEvaluationCaseListItem> Items, long TotalCount)> GetPagedAsync(
        int? clientId, string? scenario, string? search, int page, int pageSize);

    Task<IEnumerable<string>> GetScenariosAsync(int? clientId);

    Task<EmailEvaluationDataset?> GetByIdAsync(long id);

    // Inserta el caso y devuelve el id autonumérico asignado.
    Task<long> CreateAsync(EmailEvaluationDataset entity);

    Task<bool> UpdateAsync(EmailEvaluationDataset entity);

    Task<bool> DeleteAsync(long id, DateTime updatedAt, string? updatedBy);
}
