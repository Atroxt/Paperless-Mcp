using Paperless.Mcp.Brokers.Loggings;
using Paperless.Mcp.Brokers.Paperlesses;
using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Services.Foundations.Documents
{
    public partial class DocumentService : IDocumentService
    {
        private readonly IPaperlessBroker paperlessBroker;
        private readonly ILoggingBroker loggingBroker;

        public DocumentService(IPaperlessBroker paperlessBroker, ILoggingBroker loggingBroker)
        {
            this.paperlessBroker = paperlessBroker;
            this.loggingBroker = loggingBroker;
        }

        public ValueTask<Document> RetrieveDocumentByIdAsync(int documentId) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                return await this.paperlessBroker.GetDocumentByIdAsync(documentId);
            });

        public ValueTask<DocumentPage> RetrieveDocumentPageAsync(DocumentSearch documentSearch) =>
            TryCatch(async () =>
            {
                ValidateDocumentSearch(documentSearch);

                return await this.paperlessBroker.GetDocumentPageAsync(documentSearch);
            });

        public ValueTask<Document> ModifyDocumentAsync(DocumentUpdate documentUpdate) =>
            TryCatch(async () =>
            {
                ValidateDocumentUpdate(documentUpdate);

                return await this.paperlessBroker.PatchDocumentAsync(
                    documentUpdate.DocumentId,
                    MapToPaperlessDocumentUpdate(documentUpdate));
            });

        public ValueTask<string> AddDocumentFileAsync(DocumentUpload documentUpload) =>
            TryCatch(async () =>
            {
                ValidateDocumentUpload(documentUpload);

                return await this.paperlessBroker.PostDocumentFileAsync(
                    MapToPaperlessDocumentForm(documentUpload));
            });

        public ValueTask<DocumentThumbnail> RetrieveDocumentThumbnailAsync(int documentId) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                byte[] content = await this.paperlessBroker.GetDocumentThumbnailAsync(documentId);

                return new DocumentThumbnail
                {
                    DocumentId = documentId,
                    Content = content,
                };
            });

        public ValueTask<byte[]> RetrieveDocumentFileAsync(int documentId, bool original) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                return await this.paperlessBroker.GetDocumentFileAsync(documentId, original);
            });

        public ValueTask<DocumentFile> RetrieveDocumentFileWithMetadataAsync(
            int documentId,
            bool original) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                return await this.paperlessBroker.GetDocumentFileWithMetadataAsync(
                    documentId, original);
            });

        public ValueTask RemoveDocumentByIdAsync(int documentId) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                await this.paperlessBroker.DeleteDocumentByIdAsync(documentId);
            });

        public ValueTask ReprocessDocumentByIdAsync(int documentId) =>
            TryCatch(async () =>
            {
                ValidateDocumentId(documentId);

                await this.paperlessBroker.PostBulkEditAsync(new PaperlessBulkEdit
                {
                    Documents = [documentId],
                    Method = PaperlessBulkEdit.ReprocessMethod,
                });
            });

        public ValueTask ModifyDocumentsCorrespondentAsync(
            List<int> documentIds,
            int? correspondentId) =>
            TryCatch(async () =>
            {
                ValidateDocumentIds(documentIds);
                ValidateReferenceId(correspondentId, nameof(Document.Correspondent));

                await this.paperlessBroker.PostBulkEditAsync(new PaperlessBulkEdit
                {
                    Documents = documentIds,
                    Method = PaperlessBulkEdit.SetCorrespondentMethod,

                    Parameters = new Dictionary<string, object?>
                    {
                        [PaperlessBulkEdit.CorrespondentParameter] = correspondentId,
                    },
                });
            });

        public ValueTask ModifyDocumentsDocumentTypeAsync(
            List<int> documentIds,
            int? documentTypeId) =>
            TryCatch(async () =>
            {
                ValidateDocumentIds(documentIds);
                ValidateReferenceId(documentTypeId, nameof(Document.DocumentType));

                await this.paperlessBroker.PostBulkEditAsync(new PaperlessBulkEdit
                {
                    Documents = documentIds,
                    Method = PaperlessBulkEdit.SetDocumentTypeMethod,

                    Parameters = new Dictionary<string, object?>
                    {
                        [PaperlessBulkEdit.DocumentTypeParameter] = documentTypeId,
                    },
                });
            });

        public ValueTask ModifyDocumentsStoragePathAsync(
            List<int> documentIds,
            int? storagePathId) =>
            TryCatch(async () =>
            {
                ValidateDocumentIds(documentIds);
                ValidateReferenceId(storagePathId, nameof(Document.StoragePath));

                await this.paperlessBroker.PostBulkEditAsync(new PaperlessBulkEdit
                {
                    Documents = documentIds,
                    Method = PaperlessBulkEdit.SetStoragePathMethod,

                    Parameters = new Dictionary<string, object?>
                    {
                        [PaperlessBulkEdit.StoragePathParameter] = storagePathId,
                    },
                });
            });

        // modify_tags statt add_tag/remove_tag: die API verlangt beide Listen ohnehin,
        // und ein Aufruf statt zweier haelt Hinzufuegen und Entfernen in einer Transaktion.
        public ValueTask ModifyDocumentsTagsAsync(
            List<int> documentIds,
            List<int> addTagIds,
            List<int> removeTagIds) =>
            TryCatch(async () =>
            {
                ValidateDocumentIds(documentIds);
                ValidateTagIds(addTagIds, removeTagIds);

                await this.paperlessBroker.PostBulkEditAsync(new PaperlessBulkEdit
                {
                    Documents = documentIds,
                    Method = PaperlessBulkEdit.ModifyTagsMethod,

                    Parameters = new Dictionary<string, object?>
                    {
                        [PaperlessBulkEdit.AddTagsParameter] = addTagIds,
                        [PaperlessBulkEdit.RemoveTagsParameter] = removeTagIds,
                    },
                });
            });

        public ValueTask RemoveDocumentsAsync(List<int> documentIds) =>
            TryCatch(async () =>
            {
                ValidateDocumentIds(documentIds);

                await this.paperlessBroker.PostBulkEditAsync(new PaperlessBulkEdit
                {
                    Documents = documentIds,
                    Method = PaperlessBulkEdit.DeleteMethod,
                });
            });

        // Multipart kennt keine Typen: alle Zusatzangaben gehen als Zeichenkette hinaus.
        //
        // Nicht gesetzte Felder bleiben null und werden vom Broker weggelassen.
        private static PaperlessDocumentForm MapToPaperlessDocumentForm(
            DocumentUpload documentUpload) =>
            new()
            {
                Document = documentUpload.Content,
                FileName = documentUpload.FileName,
                Title = documentUpload.Title,
                Correspondent = documentUpload.CorrespondentId?.ToString(),
                DocumentType = documentUpload.DocumentTypeId?.ToString(),
                StoragePath = documentUpload.StoragePathId?.ToString(),

                Created = documentUpload.Created?.ToString(
                    "yyyy-MM-dd",
                    System.Globalization.CultureInfo.InvariantCulture),
            };

        // Geschaeftssprache in Technologiesprache: aus CorrespondentId wird
        // correspondent, aus TagIds wird tags.
        private static PaperlessDocumentUpdate MapToPaperlessDocumentUpdate(
            DocumentUpdate documentUpdate) =>
            new()
            {
                Title = documentUpdate.Title,
                Correspondent = documentUpdate.CorrespondentId,
                DocumentType = documentUpdate.DocumentTypeId,
                StoragePath = documentUpdate.StoragePathId,
                Tags = documentUpdate.TagIds,
                Created = documentUpdate.Created,
                ArchiveSerialNumber = documentUpdate.ArchiveSerialNumber,
                CustomFields = documentUpdate.CustomFields,
            };
    }
}
