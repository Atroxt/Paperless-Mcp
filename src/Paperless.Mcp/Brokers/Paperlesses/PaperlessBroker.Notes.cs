using Paperless.Mcp.Models.Foundations.Notes;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string NotesRelativeUrlFormat = "api/documents/{0}/notes/";

        public async ValueTask<List<DocumentNote>> GetDocumentNotesAsync(int documentId) =>
            await GetAsync<List<DocumentNote>>(NotesUrl(documentId));

        // Paperless antwortet auf POST und DELETE jeweils mit der vollstaendigen,
        // aktualisierten Liste des Dokuments -- nicht mit der einzelnen Notiz.
        public async ValueTask<List<DocumentNote>> PostDocumentNoteAsync(
            int documentId,
            PaperlessNote note) =>
            await PostAsync<PaperlessNote, List<DocumentNote>>(NotesUrl(documentId), note);

        // Die zu loeschende Notiz steht in der Abfragezeichenfolge, nicht im Pfad.
        public async ValueTask<List<DocumentNote>> DeleteDocumentNoteAsync(
            int documentId,
            int noteId) =>
            await DeleteAsync<List<DocumentNote>>($"{NotesUrl(documentId)}?id={noteId}");

        private static string NotesUrl(int documentId) =>
            string.Format(
                System.Globalization.CultureInfo.InvariantCulture,
                NotesRelativeUrlFormat,
                documentId);
    }
}
