using Veimen_API.Models;

namespace Veimen_API.Services;

public interface IEmailEvaluationService
{
    // Envía el caso al webhook de n8n y devuelve la respuesta cruda del flujo.
    Task<string> EvaluateAsync(EmailEvaluationDataset dataset);
}
