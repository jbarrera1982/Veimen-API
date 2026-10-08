using Veimen_API.Models;

namespace Veimen_API.Services;

public interface IClientService
{
    Task<IEnumerable<Client>> GetAllAsync();
    Task<Client?> GetByIdAsync(int id);
    Task<int> CreateAsync(Client client);
    Task<bool> UpdateAsync(int id, Client client);
    Task<bool> DeleteAsync(int id);
}
