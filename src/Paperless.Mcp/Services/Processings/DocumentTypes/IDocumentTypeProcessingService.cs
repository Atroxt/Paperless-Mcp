using Paperless.Mcp.Models.Processings.DocumentTypes;

namespace Paperless.Mcp.Services.Processings.DocumentTypes
{
    public interface IDocumentTypeProcessingService
    {
        // confirm entscheidet, ob wirklich geloescht wird. false ist der Default und
        // berichtet nur, was geschehen wuerde.
        ValueTask<DocumentTypeRemovalPlan> RemoveDocumentTypeByIdAsync(int documentTypeId, bool confirm);
    }
}
