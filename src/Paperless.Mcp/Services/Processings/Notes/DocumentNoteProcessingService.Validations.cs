using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Processings.Notes.Exceptions;

namespace Paperless.Mcp.Services.Processings.Notes
{
    public partial class DocumentNoteProcessingService
    {
        // Processing Services validieren nur, was sie selbst verwenden. Die Ids prueft
        // zwar auch die Foundation Service, aber hier wird mit ihnen vorher schon
        // gearbeitet.
        private static void ValidateOnRemove(int documentId, int noteId) =>
            Validate(
                (Rule: IsInvalid(documentId), Parameter: "DocumentId"),
                (Rule: IsInvalid(noteId), Parameter: nameof(DocumentNote.Id)));

        // Eine Notiz, die es am Dokument nicht gibt, wird nicht blind an Paperless
        // weitergereicht: dort wuerde das DELETE stillschweigend nichts tun und der
        // Aufrufer glaubte, geloescht zu haben.
        private static DocumentNote FindNote(
            List<DocumentNote> documentNotes,
            int documentId,
            int noteId)
        {
            DocumentNote? documentNote =
                documentNotes.Find(note => note.Id == noteId);

            if (documentNote is null)
            {
                var invalidDocumentNoteProcessingException =
                    new InvalidDocumentNoteProcessingException(
                        message: "Document note processing is invalid, "
                            + "fix the errors and try again.");

                invalidDocumentNoteProcessingException.UpsertDataList(
                    key: nameof(DocumentNote.Id),
                    value: $"Document {documentId} carries no note with id {noteId}");

                throw invalidDocumentNoteProcessingException;
            }

            return documentNote;
        }

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentNoteProcessingException =
                new InvalidDocumentNoteProcessingException(
                    message: "Document note processing is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentNoteProcessingException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentNoteProcessingException.ThrowIfContainsErrors();
        }
    }
}
