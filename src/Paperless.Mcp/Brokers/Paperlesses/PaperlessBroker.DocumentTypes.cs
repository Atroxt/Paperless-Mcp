using Paperless.Mcp.Models.Foundations.DocumentTypes;

namespace Paperless.Mcp.Brokers.Paperlesses
{
    public partial class PaperlessBroker
    {
        // snake_case im Pfad, anders als bei Tags und Correspondents
        private const string DocumentTypesRelativeUrl = "api/document_types/";

        public async ValueTask<DocumentType> PostDocumentTypeAsync(DocumentType documentType) =>
            await PostAsync(DocumentTypesRelativeUrl, documentType);

        public async ValueTask<DocumentType> GetDocumentTypeByIdAsync(int documentTypeId) =>
            await GetAsync<DocumentType>($"{DocumentTypesRelativeUrl}{documentTypeId}/");

        public async ValueTask<DocumentTypePage> GetDocumentTypePageAsync(int page, int pageSize) =>
            await GetAsync<DocumentTypePage>(
                $"{DocumentTypesRelativeUrl}?page={page}&page_size={pageSize}");

        public async ValueTask<DocumentType> PutDocumentTypeAsync(DocumentType documentType) =>
            await PutAsync($"{DocumentTypesRelativeUrl}{documentType.Id}/", documentType);

        public async ValueTask DeleteDocumentTypeByIdAsync(int documentTypeId) =>
            await DeleteAsync($"{DocumentTypesRelativeUrl}{documentTypeId}/");
    }
}
