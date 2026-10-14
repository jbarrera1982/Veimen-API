namespace Veimen_API.Services;

// Dispara el webhook de n8n que genera el informe de auditoría de un requerimiento.
// El webhook vive en el backend para no exponer su URL al navegador.
public interface IAuditService
{
    Task RequestAuditAsync(long requestNumber, string? email);
}
