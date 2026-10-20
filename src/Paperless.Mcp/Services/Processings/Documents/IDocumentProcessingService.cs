using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;

namespace Paperless.Mcp.Services.Processings.Documents
{
    public interface IDocumentProcessingService
    {
        ValueTask<DocumentPage> SearchDocumentsAsync(DocumentSearch documentSearch);

        // confirm entscheidet, ob wirklich gehandelt wird. false berichtet nur.
        ValueTask<DocumentRemovalPlan> RemoveDocumentByIdAsync(int documentId, bool confirm);
        ValueTask<DocumentReprocessPlan> ReprocessDocumentByIdAsync(int documentId, bool confirm);
    }
}
