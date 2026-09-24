using Veimen_API.Models;
using Veimen_API.Repositories;

namespace Veimen_API.Services;

public class PromptService : IPromptService
{
    private readonly IPromptRepository _repository;

    public PromptService(IPromptRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Prompt>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Prompt?> GetByIdAsync(long id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<long> CreateAsync(Prompt prompt)
    {
        prompt.CreatedAt = DateTime.UtcNow;
        prompt.UpdatedAt = DateTime.UtcNow;

        return await _repository.CreateAsync(prompt);
    }

    public async Task<bool> UpdateAsync(long id, Prompt prompt)
    {
        if (id != prompt.PromptId)
        {
            return false;
        }

        prompt.UpdatedAt = DateTime.UtcNow;

        return await _repository.UpdateAsync(prompt);
    }

    public async Task<bool> DeleteAsync(long id)
    {
        return await _repository.DeleteAsync(id);
    }
}
