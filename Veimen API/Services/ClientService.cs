using Veimen_API.Models;
using Veimen_API.Repositories;

namespace Veimen_API.Services;

public class ClientService : IClientService
{
    private readonly IClientRepository _repository;

    public ClientService(IClientRepository repository)
    {
        _repository = repository;
    }

    public async Task<IEnumerable<Client>> GetAllAsync()
    {
        return await _repository.GetAllAsync();
    }

    public async Task<Client?> GetByIdAsync(int id)
    {
        return await _repository.GetByIdAsync(id);
    }

    public async Task<int> CreateAsync(Client client)
    {
        Normalize(client);
        return await _repository.CreateAsync(client);
    }

    public async Task<bool> UpdateAsync(int id, Client client)
    {
        if (id != client.ClientId)
        {
            return false;
        }

        Normalize(client);
        return await _repository.UpdateAsync(client);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        return await _repository.DeleteAsync(id);
    }

    // Recorta los textos y guarda como NULL los opcionales vacíos: los emails vacíos
    // fallarían la validación [EmailAddress] del modelo.
    private static void Normalize(Client client)
    {
        client.Name = (client.Name ?? string.Empty).Trim();
        client.InboundEmail = NormalizeOptional(client.InboundEmail);
        client.OutboundEmail = NormalizeOptional(client.OutboundEmail);
        client.AnalystEmail = NormalizeOptional(client.AnalystEmail);
        client.OpenAIApiKey = NormalizeOptional(client.OpenAIApiKey);
    }

    private static string? NormalizeOptional(string? value)
    {
        var trimmed = value?.Trim();
        return string.IsNullOrEmpty(trimmed) ? null : trimmed;
    }
}
