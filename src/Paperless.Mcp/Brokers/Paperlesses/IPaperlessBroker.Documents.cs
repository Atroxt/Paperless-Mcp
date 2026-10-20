using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<Document> GetDocumentByIdAsync(int documentId);
        ValueTask<DocumentPage> GetDocumentPageAsync(DocumentSearch documentSearch);
        ValueTask<Document> PatchDocumentAsync(int documentId, PaperlessDocumentUpdate documentUpdate);
        ValueTask<string> PostDocumentFileAsync(PaperlessDocumentForm documentForm);
        ValueTask<byte[]> GetDocumentThumbnailAsync(int documentId);
        ValueTask<byte[]> GetDocumentFileAsync(int documentId, bool original);
        ValueTask<DocumentFile> GetDocumentFileWithMetadataAsync(int documentId, bool original);
        ValueTask DeleteDocumentByIdAsync(int documentId);
        ValueTask<PaperlessBulkEditResult> PostBulkEditAsync(PaperlessBulkEdit bulkEdit);
    }
}
