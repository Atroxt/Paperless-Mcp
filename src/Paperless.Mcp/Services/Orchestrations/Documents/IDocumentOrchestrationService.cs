using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Orchestrations.Documents;

namespace Paperless.Mcp.Services.Orchestrations.Documents
{
    public interface IDocumentOrchestrationService
    {
        // Laedt hoch und wartet, bis Paperless damit durch ist -- oder bis die Wartezeit
        // um ist. Beides ist ein gueltiges Ergebnis, siehe DocumentUploadResult.
        ValueTask<DocumentUploadResult> AddDocumentAndAwaitAsync(
            DocumentUpload documentUpload,
            int timeoutSeconds);
    }
}
