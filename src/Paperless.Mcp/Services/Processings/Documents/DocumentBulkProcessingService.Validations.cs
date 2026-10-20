using Paperless.Mcp.Models.Processings.Documents;
using Paperless.Mcp.Models.Processings.Documents.Exceptions;

namespace Paperless.Mcp.Services.Processings.Documents
{
    public partial class DocumentBulkProcessingService
    {
        // Diese Schicht prueft nur, was sie selbst verwendet: dass ueberhaupt etwas
        // geaendert werden soll und dass die Dreiwertigkeit eindeutig ist. Ob die Ids
        // gueltig sind, prueft die Foundation Service.
        private static void ValidateDocumentBulkUpdate(DocumentBulkUpdate documentBulkUpdate)
        {
            ValidateDocumentBulkUpdateIsNotNull(documentBulkUpdate);
            ValidateDocumentIdsOnAct(documentBulkUpdate.DocumentIds);

            Validate(
                (Rule: ChangesNothing(documentBulkUpdate),
                    Parameter: nameof(DocumentBulkUpdate)),

                (Rule: IsAmbiguous(documentBulkUpdate.Correspondent),
                    Parameter: nameof(DocumentBulkUpdate.Correspondent)),

                (Rule: IsAmbiguous(documentBulkUpdate.DocumentType),
                    Parameter: nameof(DocumentBulkUpdate.DocumentType)),

                (Rule: IsAmbiguous(documentBulkUpdate.StoragePath),
                    Parameter: nameof(DocumentBulkUpdate.StoragePath)),

                (Rule: OverlapsTagIds(documentBulkUpdate),
                    Parameter: nameof(DocumentBulkUpdate.AddTagIds)));
        }

        private static void ValidateDocumentIdsOnAct(List<int> documentIds)
        {
            if (documentIds is null)
            {
                throw new NullDocumentProcessingException(message: "DocumentIds is null.");
            }

            Validate((Rule: IsEmpty(documentIds), Parameter: nameof(DocumentBulkUpdate.DocumentIds)));
        }

        private static void ValidateDocumentBulkUpdateIsNotNull(
            DocumentBulkUpdate documentBulkUpdate)
        {
            if (documentBulkUpdate is null)
            {
                throw new NullDocumentProcessingException(
                    message: "DocumentBulkUpdate is null.");
            }
        }

        // Eine Sammeloperation ohne Aenderung waere der gefaehrlichste Leerlauf von allen:
        // der Aufrufer bekaeme einen Bericht ueber betroffene Dokumente zu sehen und
        // hielte eine Aenderung fuer geschehen.
        private static dynamic ChangesNothing(DocumentBulkUpdate documentBulkUpdate) => new
        {
            Condition = documentBulkUpdate.Correspondent is null
                && documentBulkUpdate.DocumentType is null
                && documentBulkUpdate.StoragePath is null
                && documentBulkUpdate.AddTagIds.Count is 0
                && documentBulkUpdate.RemoveTagIds.Count is 0,

            Message = "At least one change must be requested",
        };

        // Setzen und Leeren zugleich ist keine Absicht, sondern ein Irrtum.
        private static dynamic IsAmbiguous(DocumentReferenceUpdate? referenceUpdate) => new
        {
            Condition = referenceUpdate is not null
                && referenceUpdate.Clear
                && referenceUpdate.Id is not null,

            Message = "Either set an id or clear the reference, not both",
        };

        // Derselbe Tag in beiden Listen: Paperless entscheidet das still, wir nicht.
        private static dynamic OverlapsTagIds(DocumentBulkUpdate documentBulkUpdate) => new
        {
            Condition = documentBulkUpdate.AddTagIds
                .Intersect(documentBulkUpdate.RemoveTagIds)
                .Any(),

            Message = "A tag must not be added and removed in the same call",
        };

        private static dynamic IsEmpty(List<int> ids) => new
        {
            Condition = ids.Count is 0,
            Message = "At least one document id is required",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentProcessingException =
                new InvalidDocumentProcessingException(
                    message: "Document processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentProcessingException.ThrowIfContainsErrors();
        }
    }
}
