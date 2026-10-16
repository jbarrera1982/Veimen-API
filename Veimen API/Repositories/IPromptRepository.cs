using Veimen_API.Models;

namespace Veimen_API.Repositories;

public interface IPromptRepository
{
    Task<IEnumerable<Prompt>> GetAllAsync(int? clientId);
    Task<Prompt?> GetByIdAsync(long id);
    Task<long> CreateAsync(Prompt prompt);
    Task<bool> UpdateAsync(Prompt prompt);
    Task<bool> DeleteAsync(long id);
}
