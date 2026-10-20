using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Services.Foundations.DocumentTypes
{
    // Foundation Service: uebersetzt Technologiesprache in Geschaeftssprache
    // (Post -> Add, Get -> Retrieve, Put -> Modify, Delete -> Remove) und ist die
    // letzte Abstraktionsschicht vor der eigentlichen Geschaeftslogik.
    public interface IDocumentTypeService
    {
        ValueTask<DocumentType> AddDocumentTypeAsync(DocumentType documentType);
        ValueTask<DocumentType> RetrieveDocumentTypeByIdAsync(int documentTypeId);
        ValueTask<DocumentTypePage> RetrieveDocumentTypePageAsync(int page, int pageSize);
        ValueTask<DocumentType> ModifyDocumentTypeAsync(DocumentType documentType);
        ValueTask RemoveDocumentTypeByIdAsync(int documentTypeId);
    }
}
