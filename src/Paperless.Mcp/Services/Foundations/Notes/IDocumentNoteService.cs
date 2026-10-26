using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Services.Foundations.Notes
{
    // Foundation Service: uebersetzt Technologiesprache in Geschaeftssprache
    // (Get -> Retrieve, Post -> Add, Delete -> Remove) und ist die letzte
    // Abstraktionsschicht vor der eigentlichen Geschaeftslogik.
    //
    // Ein Modify fehlt bewusst: die Paperless-API kennt kein Aendern von Notizen.
    public interface IDocumentNoteService
    {
        ValueTask<List<DocumentNote>> RetrieveDocumentNotesAsync(int documentId);
        ValueTask<List<DocumentNote>> AddDocumentNoteAsync(int documentId, string note);
        ValueTask<List<DocumentNote>> RemoveDocumentNoteAsync(int documentId, int noteId);
    }
}
