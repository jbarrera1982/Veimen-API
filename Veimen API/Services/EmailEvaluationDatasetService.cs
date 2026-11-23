using Veimen_API.Helpers;
using Veimen_API.Models;
using Veimen_API.Models.Dtos;
using Veimen_API.Repositories;

namespace Veimen_API.Services;

public class EmailEvaluationDatasetService : IEmailEvaluationDatasetService
{
    private readonly IEmailEvaluationDatasetRepository _repository;

    public EmailEvaluationDatasetService(IEmailEvaluationDatasetRepository repository)
    {
        _repository = repository;
    }

    public async Task<PagedResult<EmailEvaluationCaseListItem>> GetPagedAsync(
        int? clientId, string? scenario, string? search, int page, int pageSize)
    {
        var (items, totalCount) = await _repository.GetPagedAsync(
            clientId, scenario, search, page, pageSize);

        // El repositorio trae un recorte del HTML; acá se convierte en texto plano
        // para que el listado no transporte el contenido completo.
        foreach (var item in items)
        {
            item.ContentPreview = HtmlTextConverter.ToPreview(item.ContentPreview);
        }

        return new PagedResult<EmailEvaluationCaseListItem>
        {
            Items = items,
            TotalCount = totalCount,
            Page = page,
            PageSize = pageSize,
        };
    }

    public Task<IEnumerable<string>> GetScenariosAsync(int? clientId)
    {
        return _repository.GetScenariosAsync(clientId);
    }

    public Task<EmailEvaluationDataset?> GetByIdAsync(long id)
    {
        return _repository.GetByIdAsync(id);
    }

    public Task<long> CreateAsync(EmailEvaluationDataset entity, string? userEmail)
    {
        var now = DateTime.UtcNow;
        entity.CreatedAt = now;
        entity.UpdatedAt = now;
        entity.CreatedBy = userEmail;
        entity.UpdatedBy = userEmail;

        return _repository.CreateAsync(entity);
    }

    public Task<bool> UpdateAsync(EmailEvaluationDataset entity, string? userEmail)
    {
        entity.UpdatedAt = DateTime.UtcNow;
        entity.UpdatedBy = userEmail;

        return _repository.UpdateAsync(entity);
    }

    public Task<bool> DeleteAsync(long id, string? userEmail)
    {
        return _repository.DeleteAsync(id, DateTime.UtcNow, userEmail);
    }
}
