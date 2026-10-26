using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial interface IPaperlessBroker
    {
        ValueTask<List<DocumentNote>> GetDocumentNotesAsync(int documentId);
        ValueTask<List<DocumentNote>> PostDocumentNoteAsync(int documentId, PaperlessNote note);
        ValueTask<List<DocumentNote>> DeleteDocumentNoteAsync(int documentId, int noteId);
    }
}
