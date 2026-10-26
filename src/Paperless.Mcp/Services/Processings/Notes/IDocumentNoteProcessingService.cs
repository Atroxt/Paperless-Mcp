using Paperless.Mcp.Models.Processings.Notes;

namespace Paperless.Mcp.Services.Processings.Notes
{
    public interface IDocumentNoteProcessingService
    {
        // confirm entscheidet, ob wirklich geloescht wird. false ist der Default und
        // berichtet nur, was geschehen wuerde.
        ValueTask<DocumentNoteRemovalPlan> RemoveDocumentNoteAsync(
            int documentId,
            int noteId,
            bool confirm);
    }
}
