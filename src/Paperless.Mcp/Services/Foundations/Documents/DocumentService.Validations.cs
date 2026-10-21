using Paperless.Mcp.Models.Foundations.Documents;
using Paperless.Mcp.Models.Foundations.Documents.Exceptions;

namespace Paperless.Mcp.Services.Foundations.Documents
{
    public partial class DocumentService
    {
        // Paperless sortiert nach diesen Feldern. Bewusst eine geschlossene Liste:
        // ein unbekannter Wert wird von der API stillschweigend ignoriert, die Antwort
        // waere dann anders sortiert als angefragt, ohne dass es jemand merkt.
        private static readonly HashSet<string> AllowedOrderingFields =
            new(StringComparer.OrdinalIgnoreCase)
            {
                "id", "title", "created", "added", "modified",
                "correspondent__name", "document_type__name", "archive_serial_number",
            };

        public const int MaxDocumentsPerBulkCall = 100;

        private static void ValidateDocumentId(int documentId) =>
            Validate((Rule: IsInvalid(documentId), Parameter: nameof(Document.Id)));

        private static void ValidateDocumentSearch(DocumentSearch documentSearch)
        {
            ValidateDocumentSearchIsNotNull(documentSearch);

            Validate(
                (Rule: IsInvalid(documentSearch.Page), Parameter: nameof(DocumentSearch.Page)),
                (Rule: IsInvalid(documentSearch.PageSize), Parameter: nameof(DocumentSearch.PageSize)),
                (Rule: IsInvalidOrdering(documentSearch.Ordering), Parameter: nameof(DocumentSearch.Ordering)),
                (Rule: IsReversedRange(documentSearch), Parameter: nameof(DocumentSearch.CreatedFrom)));
        }

        private static void ValidateDocumentUpload(DocumentUpload documentUpload)
        {
            ValidateDocumentUploadIsNotNull(documentUpload);

            Validate(
                (Rule: IsInvalid(documentUpload.FileName), Parameter: nameof(DocumentUpload.FileName)),
                (Rule: IsEmpty(documentUpload.Content, documentUpload.ContentStream),
                    Parameter: nameof(DocumentUpload.Content)));
        }

        private static void ValidateDocumentUploadIsNotNull(DocumentUpload documentUpload)
        {
            if (documentUpload is null)
            {
                throw new NullDocumentException(message: "DocumentUpload is null.");
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        private static dynamic IsEmpty(byte[] content, Stream? contentStream) => new
        {
            Condition = contentStream is null && (content is null || content.Length is 0),
            Message = "File content is required",
        };

        private static void ValidateDocumentUpdate(DocumentUpdate documentUpdate)
        {
            ValidateDocumentUpdateIsNotNull(documentUpdate);

            Validate(
                (Rule: IsInvalid(documentUpdate.DocumentId), Parameter: nameof(DocumentUpdate.DocumentId)),
                (Rule: ChangesNothing(documentUpdate), Parameter: nameof(DocumentUpdate)),
                (Rule: IsInvalidCustomFieldReference(documentUpdate), Parameter: nameof(DocumentUpdate.CustomFields)));
        }

        private static void ValidateDocumentUpdateIsNotNull(DocumentUpdate documentUpdate)
        {
            if (documentUpdate is null)
            {
                throw new NullDocumentException(message: "DocumentUpdate is null.");
            }
        }

        // Eine Aenderung ohne Aenderung waere ein stiller Leerlauf: der Aufrufer bekaeme
        // das unveraenderte Dokument zurueck und hielte es fuer einen Erfolg.
        private static dynamic ChangesNothing(DocumentUpdate documentUpdate) => new
        {
            Condition = documentUpdate.Title is null
                && documentUpdate.CorrespondentId is null
                && documentUpdate.DocumentTypeId is null
                && documentUpdate.StoragePathId is null
                && documentUpdate.TagIds is null
                && documentUpdate.Created is null
                && documentUpdate.ArchiveSerialNumber is null
                && documentUpdate.CustomFields is null,

            Message = "At least one field must be set",
        };

        private static dynamic IsInvalidCustomFieldReference(DocumentUpdate documentUpdate) => new
        {
            Condition = documentUpdate.CustomFields?.Any(value => value.Field <= 0) is true,
            Message = "Every custom field value must reference a field id greater than zero",
        };

        private static void ValidateDocumentIds(List<int> documentIds)
        {
            if (documentIds is null)
            {
                throw new NullDocumentException(message: "DocumentIds is null.");
            }

            // Eine leere Liste darf nicht durchgehen: bulk_edit antwortet darauf mit
            // 200 OK, ohne etwas zu tun -- an der Instanz geprueft. Der Aufrufer haelte
            // den Leerlauf fuer einen Erfolg.
            Validate(
                (Rule: IsEmpty(documentIds), Parameter: nameof(PaperlessBulkEdit.Documents)),
                (Rule: ContainsInvalidId(documentIds), Parameter: nameof(PaperlessBulkEdit.Documents)),
                (Rule: IsTooLarge(documentIds), Parameter: nameof(PaperlessBulkEdit.Documents)));
        }

        // null ist hier gueltig und heisst "Zuordnung leeren".
        private static void ValidateReferenceId(int? referenceId, string parameter) =>
            Validate((Rule: IsInvalidReference(referenceId), Parameter: parameter));

        private static void ValidateTagIds(List<int> addTagIds, List<int> removeTagIds)
        {
            if (addTagIds is null || removeTagIds is null)
            {
                throw new NullDocumentException(message: "TagIds is null.");
            }

            Validate(
                (Rule: ChangesNoTag(addTagIds, removeTagIds),
                    Parameter: nameof(PaperlessBulkEdit.Parameters)),

                (Rule: ContainsInvalidId(addTagIds),
                    Parameter: PaperlessBulkEdit.AddTagsParameter),

                (Rule: ContainsInvalidId(removeTagIds),
                    Parameter: PaperlessBulkEdit.RemoveTagsParameter));
        }

        private static dynamic IsEmpty(List<int> ids) => new
        {
            Condition = ids.Count is 0,
            Message = "At least one id is required",
        };

        private static dynamic ContainsInvalidId(List<int> ids) => new
        {
            Condition = ids.Any(id => id <= 0),
            Message = "Every id must be greater than zero",
        };

        // Obergrenze fuer eine Sammeloperation. Sie begrenzt die Tragweite eines einzelnen
        // Aufrufs und sorgt zugleich dafuer, dass sich die betroffenen Dokumente in einer
        // einzigen Seite nachlesen lassen -- das braucht der Bericht der Processing Service.
        private static dynamic IsTooLarge(List<int> ids) => new
        {
            Condition = ids.Count > MaxDocumentsPerBulkCall,
            Message = $"At most {MaxDocumentsPerBulkCall} documents per call",
        };

        private static dynamic IsInvalidReference(int? referenceId) => new
        {
            Condition = referenceId is not null && referenceId <= 0,
            Message = "Value must be greater than zero, or null to clear the reference",
        };

        private static dynamic ChangesNoTag(List<int> addTagIds, List<int> removeTagIds) => new
        {
            Condition = addTagIds.Count is 0 && removeTagIds.Count is 0,
            Message = "At least one tag must be added or removed",
        };

        private static void ValidateDocumentSearchIsNotNull(DocumentSearch documentSearch)
        {
            if (documentSearch is null)
            {
                throw new NullDocumentException(message: "DocumentSearch is null.");
            }
        }

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static dynamic IsInvalidOrdering(string? ordering) => new
        {
            Condition = string.IsNullOrWhiteSpace(ordering) is false
                && AllowedOrderingFields.Contains(ordering.TrimStart('-')) is false,

            Message =
                "Ordering must be one of "
                + string.Join(", ", AllowedOrderingFields.Order(StringComparer.Ordinal))
                + ", optionally prefixed with - for descending",
        };

        private static dynamic IsReversedRange(DocumentSearch documentSearch) => new
        {
            Condition = documentSearch.CreatedFrom is not null
                && documentSearch.CreatedTo is not null
                && documentSearch.CreatedFrom > documentSearch.CreatedTo,

            Message = "CreatedFrom must not be later than CreatedTo",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentException =
                new InvalidDocumentException(
                    message: "Document is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentException.ThrowIfContainsErrors();
        }
    }
}
