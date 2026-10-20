using Paperless.Mcp.Models.Foundations.Documents;

namespace Paperless.Mcp.Services.Foundations.Documents
{
    public interface IDocumentService
    {
        ValueTask<Document> RetrieveDocumentByIdAsync(int documentId);
        ValueTask<DocumentPage> RetrieveDocumentPageAsync(DocumentSearch documentSearch);
        ValueTask<Document> ModifyDocumentAsync(DocumentUpdate documentUpdate);
        ValueTask<string> AddDocumentFileAsync(DocumentUpload documentUpload);
        ValueTask<DocumentThumbnail> RetrieveDocumentThumbnailAsync(int documentId);
        ValueTask<byte[]> RetrieveDocumentFileAsync(int documentId, bool original);
        ValueTask<DocumentFile> RetrieveDocumentFileWithMetadataAsync(int documentId, bool original);
        ValueTask RemoveDocumentByIdAsync(int documentId);
        ValueTask ReprocessDocumentByIdAsync(int documentId);

        // Sammeloperationen. Jede ist genau ein bulk_edit-Aufruf; mehrere Aenderungen an
        // denselben Dokumenten sind mehrere Aufrufe, und das Zusammensetzen gehoert eine
        // Schicht hoeher.
        //
        // `null` heisst hier LEEREN, nicht "nicht aendern" -- so verlangt es die API, und
        // die Dreiwertigkeit (unberuehrt / setzen / leeren) loest die Processing Service.
        ValueTask ModifyDocumentsCorrespondentAsync(List<int> documentIds, int? correspondentId);
        ValueTask ModifyDocumentsDocumentTypeAsync(List<int> documentIds, int? documentTypeId);
        ValueTask ModifyDocumentsStoragePathAsync(List<int> documentIds, int? storagePathId);

        ValueTask ModifyDocumentsTagsAsync(
            List<int> documentIds,
            List<int> addTagIds,
            List<int> removeTagIds);

        ValueTask RemoveDocumentsAsync(List<int> documentIds);
    }
}
