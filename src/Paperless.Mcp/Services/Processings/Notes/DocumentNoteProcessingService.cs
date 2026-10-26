using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Processings.Notes;
using Paperless.Mcp.Services.Foundations.Notes;

namespace Paperless.Mcp.Services.Processings.Notes
{
    // Hoeherwertige Logik einer Entity: holt erst die Notizen des Dokuments, um die
    // Tragweite zu kennen, und entfernt nur bei ausdruecklicher Bestaetigung. Zwei
    // Schritte auf einer Foundation Service -- genau das, wofuer die Processing-Schicht
    // da ist.
    public partial class DocumentNoteProcessingService : IDocumentNoteProcessingService
    {
        // So viel Text zeigt der Bericht. Eine Notiz darf 10000 Zeichen lang sein, und
        // die gehoeren nicht ungekuerzt in eine Rueckfrage an den Benutzer.
        private const int SummaryNoteLength = 120;

        private readonly IDocumentNoteService documentNoteService;

        public DocumentNoteProcessingService(IDocumentNoteService documentNoteService) =>
            this.documentNoteService = documentNoteService;

        public ValueTask<DocumentNoteRemovalPlan> RemoveDocumentNoteAsync(
            int documentId,
            int noteId,
            bool confirm) =>
            TryCatch(async () =>
            {
                ValidateOnRemove(documentId, noteId);

                // Immer zuerst lesen: der Bericht soll nennen, welche Notiz betroffen
                // ist, und zwar auch dann, wenn anschliessend geloescht wird.
                List<DocumentNote> documentNotes =
                    await this.documentNoteService.RetrieveDocumentNotesAsync(documentId);

                DocumentNote targetNote = FindNote(documentNotes, documentId, noteId);

                if (confirm is false)
                {
                    return CreateDryRunPlan(targetNote, documentNotes.Count);
                }

                await this.documentNoteService.RemoveDocumentNoteAsync(documentId, noteId);

                return CreateRemovedPlan(targetNote, documentNotes.Count);
            });

        private static DocumentNoteRemovalPlan CreateDryRunPlan(
            DocumentNote documentNote,
            int noteCount) =>
            new()
            {
                DocumentId = documentNote.Document,
                NoteId = documentNote.Id,
                Note = Shorten(documentNote.Note),
                NoteCount = noteCount,
                Removed = false,

                Summary =
                    $"Dry run: note {documentNote.Id} on document {documentNote.Document} "
                    + $"would be deleted. Its text is: \"{Shorten(documentNote.Note)}\". "
                    + $"{DescribeNoteCount(noteCount)} "
                    + "Call again with confirm set to true to actually delete it.",
            };

        private static DocumentNoteRemovalPlan CreateRemovedPlan(
            DocumentNote documentNote,
            int noteCount) =>
            new()
            {
                DocumentId = documentNote.Document,
                NoteId = documentNote.Id,
                Note = Shorten(documentNote.Note),
                NoteCount = noteCount,
                Removed = true,

                Summary =
                    $"Note {documentNote.Id} on document {documentNote.Document} was "
                    + $"deleted. Its text was: \"{Shorten(documentNote.Note)}\". "
                    + $"{DescribeNoteCount(noteCount - 1)}",
            };

        private static string DescribeNoteCount(int noteCount) =>
            noteCount is 1
                ? "The document then carries 1 note."
                : $"The document then carries {noteCount} notes.";

        private static string Shorten(string note) =>
            note.Length <= SummaryNoteLength
                ? note
                : $"{note[..SummaryNoteLength]}...";
    }
}
