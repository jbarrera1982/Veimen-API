namespace Veimen_API.Models.Dtos;

// Resultados de usage/costs que traen un project_id de OpenAI. El backend les agrega el
// nombre del cliente dueño del proyecto (`project_name`) para que el frontend no tenga que
// resolverlo ni depender del permiso clients.read (un usuario con cliente asociado puede no
// tenerlo y, aun así, debe ver el nombre de su proyecto).
public interface IOpenAiProjectResult
{
    string? ProjectId { get; set; }

    string? ProjectName { get; set; }
}
