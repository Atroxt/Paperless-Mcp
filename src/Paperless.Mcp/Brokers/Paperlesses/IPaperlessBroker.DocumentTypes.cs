using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<DocumentType> PostDocumentTypeAsync(DocumentType documentType);
        ValueTask<DocumentType> GetDocumentTypeByIdAsync(int documentTypeId);
        ValueTask<DocumentTypePage> GetDocumentTypePageAsync(int page, int pageSize);
        ValueTask<DocumentType> PutDocumentTypeAsync(DocumentType documentType);
        ValueTask DeleteDocumentTypeByIdAsync(int documentTypeId);
    }
}
