using Veimen_API.Models;

namespace Veimen_API.Services;

public interface IPromptService
{
    Task<IEnumerable<Prompt>> GetAllAsync();
    Task<Prompt?> GetByIdAsync(long id);
    Task<long> CreateAsync(Prompt prompt);
    Task<bool> UpdateAsync(long id, Prompt prompt);
    Task<bool> DeleteAsync(long id);
}
