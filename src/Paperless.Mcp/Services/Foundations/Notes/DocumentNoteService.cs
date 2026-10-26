using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Services.Foundations.Notes
{
    public partial class DocumentNoteService : IDocumentNoteService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public DocumentNoteService(
            IPaperlessBroker paperlessBroker,
            ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<List<DocumentNote>> RetrieveDocumentNotesAsync(int documentId) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                List<DocumentNote> documentNotes =
                    await this.paperlessBroker.GetDocumentNotesAsync(documentId);

                return AssignDocument(documentNotes, documentId);
            });

        public ValueTask<List<DocumentNote>> AddDocumentNoteAsync(int documentId, string note) =>
            TryCatch(async () =>
            {
                ValidateDocumentNoteOnAdd(documentId, note);

                List<DocumentNote> documentNotes =
                    await this.paperlessBroker.PostDocumentNoteAsync(
                        documentId,
                        new PaperlessNote { Note = note });

                return AssignDocument(documentNotes, documentId);
            });

        public ValueTask<List<DocumentNote>> RemoveDocumentNoteAsync(int documentId, int noteId) =>
            TryCatch(async () =>
            {
                ValidateDocumentNoteOnRemove(documentId, noteId);

                List<DocumentNote> documentNotes =
                    await this.paperlessBroker.DeleteDocumentNoteAsync(documentId, noteId);

                return AssignDocument(documentNotes, documentId);
            });

        // Paperless gibt das Dokument nicht mit zurueck -- es steht im Pfad. Hier
        // nachgetragen, damit eine einzelne Notiz fuer sich selbst sprechen kann.
        private static List<DocumentNote> AssignDocument(
            List<DocumentNote> documentNotes,
            int documentId)
        {
            foreach (DocumentNote documentNote in documentNotes)
            {
                documentNote.Document = documentId;
            }

            return documentNotes;
        }
    }
}
