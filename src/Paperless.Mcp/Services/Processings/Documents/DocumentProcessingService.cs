using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Services.Foundations.Documents;

namespace Paperless.Mcp.Services.Processings.Documents
{
    // Hoeherwertige Logik einer Entity: begrenzt die Seitengroesse, bevor die Anfrage
    // ueberhaupt gestellt wird. Genau eine Foundation Service als Abhaengigkeit.
    public partial class DocumentProcessingService : IDocumentProcessingService
    {
        private readonly IDocumentService documentService;
        private readonly DocumentProcessingConfigurations configurations;

        public DocumentProcessingService(
            IDocumentService documentService,
            DocumentProcessingConfigurations configurations)
        {
            this.documentService = documentService;
            this.configurations = configurations;
        }

        public ValueTask<DocumentPage> SearchDocumentsAsync(DocumentSearch documentSearch) =>
            TryCatch(async () =>
            {
                ValidateDocumentSearchOnSearch(documentSearch);

                DocumentSearch clampedSearch = ClampPageSize(documentSearch);

                return await this.documentService.RetrieveDocumentPageAsync(clampedSearch);
            });

        public ValueTask<DocumentRemovalPlan> RemoveDocumentByIdAsync(
            int documentId,
            bool confirm) =>
            TryCatch(async () =>
            {
                ValidateDocumentIdOnAct(documentId);

                // Erst lesen: der Bericht soll sagen, WELCHES Dokument gemeint ist.
                Document document =
                    await this.documentService.RetrieveDocumentByIdAsync(documentId);

                if (confirm is false)
                {
                    return new DocumentRemovalPlan
                    {
                        DocumentId = document.Id,
                        Title = document.Title,
                        Removed = false,

                        Summary =
                            $"Dry run: document '{document.Title}' would be deleted, "
                            + "including its file and extracted text. This cannot be "
                            + "undone. Call again with confirm set to true to delete it.",
                    };
                }

                await this.documentService.RemoveDocumentByIdAsync(documentId);

                return new DocumentRemovalPlan
                {
                    DocumentId = document.Id,
                    Title = document.Title,
                    Removed = true,
                    Summary = $"Document '{document.Title}' was deleted.",
                };
            });

        public ValueTask<DocumentReprocessPlan> ReprocessDocumentByIdAsync(
            int documentId,
            bool confirm) =>
            TryCatch(async () =>
            {
                ValidateDocumentIdOnAct(documentId);

                Document document =
                    await this.documentService.RetrieveDocumentByIdAsync(documentId);

                if (confirm is false)
                {
                    return new DocumentReprocessPlan
                    {
                        DocumentId = document.Id,
                        Title = document.Title,
                        Started = false,

                        Summary =
                            $"Dry run: document '{document.Title}' would be read again. "
                            + "Its extracted text is regenerated, so manual corrections "
                            + "to the text are lost. Call again with confirm set to true "
                            + "to start it.",
                    };
                }

                await this.documentService.ReprocessDocumentByIdAsync(documentId);

                return new DocumentReprocessPlan
                {
                    DocumentId = document.Id,
                    Title = document.Title,
                    Started = true,

                    Summary =
                        $"Reprocessing of document '{document.Title}' was started. "
                        + "Paperless runs it in the background.",
                };
            });

        // Bewusst eine Kopie statt den Eingabewert zu veraendern: wer sein Suchmodell
        // wiederverwendet, soll es unveraendert vorfinden.
        private DocumentSearch ClampPageSize(DocumentSearch documentSearch) =>
            new()
            {
                Query = documentSearch.Query,
                TitleContains = documentSearch.TitleContains,
                CorrespondentId = documentSearch.CorrespondentId,
                DocumentTypeId = documentSearch.DocumentTypeId,
                StoragePathId = documentSearch.StoragePathId,
                TagIds = documentSearch.TagIds,
                DocumentIds = documentSearch.DocumentIds,
                CreatedFrom = documentSearch.CreatedFrom,
                CreatedTo = documentSearch.CreatedTo,
                Ordering = documentSearch.Ordering,
                Page = documentSearch.Page,

                PageSize = Math.Min(
                    documentSearch.PageSize,
                    this.configurations.MaxPageSize),
            };
    }
}
