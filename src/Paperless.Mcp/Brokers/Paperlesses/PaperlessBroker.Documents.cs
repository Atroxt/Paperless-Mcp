using System.Globalization;
using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        private const string DocumentsRelativeUrl = "api/documents/";

        public async ValueTask<Document> GetDocumentByIdAsync(int documentId) =>
            await GetAsync<Document>($"{DocumentsRelativeUrl}{documentId}/");

        public async ValueTask<DocumentPage> GetDocumentPageAsync(DocumentSearch documentSearch) =>
            await GetAsync<DocumentPage>(
                $"{DocumentsRelativeUrl}?{BuildQuery(documentSearch)}");

        public async ValueTask<Document> PatchDocumentAsync(
            int documentId,
            PaperlessDocumentUpdate documentUpdate) =>
            await PatchAsync<PaperlessDocumentUpdate, Document>(
                $"{DocumentsRelativeUrl}{documentId}/",
                documentUpdate);

        // Antwort ist die Kennung des Verarbeitungsauftrags als JSON-Zeichenkette;
        // das Dokument entsteht erst danach.
        public async ValueTask<string> PostDocumentFileAsync(PaperlessDocumentForm documentForm) =>
            await PostFileAsync<string>(
                relativeUrl: $"{DocumentsRelativeUrl}post_document/",
                fileContent: documentForm.Document,
                fileStream: documentForm.DocumentStream,
                fileName: documentForm.FileName,
                fields: BuildFormFields(documentForm));

        // Nicht gesetzte Angaben werden weggelassen, nicht leer gesendet.
        private static Dictionary<string, string> BuildFormFields(
            PaperlessDocumentForm documentForm)
        {
            var fields = new Dictionary<string, string>();

            Add("title", documentForm.Title);
            Add("correspondent", documentForm.Correspondent);
            Add("document_type", documentForm.DocumentType);
            Add("storage_path", documentForm.StoragePath);
            Add("created", documentForm.Created);

            return fields;

            void Add(string name, string? value)
            {
                if (string.IsNullOrWhiteSpace(value) is false)
                {
                    fields.Add(name, value);
                }
            }
        }

        public async ValueTask<byte[]> GetDocumentThumbnailAsync(int documentId) =>
            await GetBytesAsync($"{DocumentsRelativeUrl}{documentId}/thumb/");

        // original=true liefert die Originaldatei, sonst die Archivfassung.
        public async ValueTask<byte[]> GetDocumentFileAsync(int documentId, bool original) =>
            await GetBytesAsync(
                $"{DocumentsRelativeUrl}{documentId}/download/?original={(original ? "true" : "false")}");

        public async ValueTask<DocumentFile> GetDocumentFileWithMetadataAsync(
            int documentId,
            bool original)
        {
            (byte[] content, string contentType, string fileName) = await GetFileAsync(
                $"{DocumentsRelativeUrl}{documentId}/download/?original={(original ? "true" : "false")}");

            return new DocumentFile
            {
                DocumentId = documentId,
                Content = content,
                ContentType = contentType,
                FileName = fileName,
            };
        }

        public async ValueTask DeleteDocumentByIdAsync(int documentId) =>
            await DeleteAsync($"{DocumentsRelativeUrl}{documentId}/");

        public async ValueTask<PaperlessBulkEditResult> PostBulkEditAsync(
            PaperlessBulkEdit bulkEdit) =>
            await PostAsync<PaperlessBulkEdit, PaperlessBulkEditResult>(
                $"{DocumentsRelativeUrl}bulk_edit/",
                bulkEdit);

        // Uebersetzt das Suchmodell in die Abfragesprache von Paperless. Das ist
        // Technologiesprache und gehoert damit in den Broker -- die Bedingungen hier
        // entscheiden nichts fachlich, sie lassen nur weg, was nicht gesetzt ist.
        private static string BuildQuery(DocumentSearch documentSearch)
        {
            var parameters = new List<string>
            {
                $"page={documentSearch.Page}",
                $"page_size={documentSearch.PageSize}",
            };

            Add("query", documentSearch.Query);
            Add("title__icontains", documentSearch.TitleContains);
            Add("correspondent__id", documentSearch.CorrespondentId?.ToString());
            Add("document_type__id", documentSearch.DocumentTypeId?.ToString());
            Add("storage_path__id", documentSearch.StoragePathId?.ToString());
            Add("ordering", documentSearch.Ordering);

            Add("created__date__gte", FormatDate(documentSearch.CreatedFrom));
            Add("created__date__lte", FormatDate(documentSearch.CreatedTo));

            if (documentSearch.TagIds.Count > 0)
            {
                Add("tags__id__all", string.Join(",", documentSearch.TagIds));
            }

            if (documentSearch.DocumentIds.Count > 0)
            {
                Add("id__in", string.Join(",", documentSearch.DocumentIds));
            }

            return string.Join("&", parameters);

            void Add(string name, string? value)
            {
                if (string.IsNullOrWhiteSpace(value) is false)
                {
                    parameters.Add($"{name}={Uri.EscapeDataString(value)}");
                }
            }
        }

        private static string? FormatDate(DateOnly? date) =>
            date?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
    }
}
