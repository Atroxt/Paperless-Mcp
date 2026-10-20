using Paperless.Mcp.Models.Processings.Documents;

namespace Paperless.Mcp.Services.Processings.Documents
{
    public interface IDocumentBulkProcessingService
    {
        // confirm entscheidet, ob wirklich gehandelt wird. false berichtet nur.
        ValueTask<DocumentBulkPlan> BulkUpdateDocumentsAsync(
            DocumentBulkUpdate documentBulkUpdate,
            bool confirm);

        ValueTask<DocumentBulkPlan> BulkRemoveDocumentsAsync(
            List<int> documentIds,
            bool confirm);
    }
}
