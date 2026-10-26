using Paperless.Mcp.Models.Foundations.Notes;
using Paperless.Mcp.Models.Foundations.Notes.Exceptions;

namespace Paperless.Mcp.Services.Foundations.Notes
{
    public partial class DocumentNoteService
    {
        // Paperless speichert den Notiztext als TextField ohne harte Obergrenze. Die
        // Schranke hier schuetzt davor, dass ein Assistent versehentlich ein ganzes
        // Dokument als Notiz ablegt.
        private const int NoteMaximumLength = 10_000;

        private static void ValidateDocumentId(int documentId) =>
            Validate((Rule: IsInvalid(documentId), Parameter: "DocumentId"));

        private static void ValidateDocumentNoteOnAdd(int documentId, string note)
        {
            ValidateNoteIsNotNull(note);

            Validate(
                (Rule: IsInvalid(documentId), Parameter: "DocumentId"),
                (Rule: IsInvalid(note), Parameter: nameof(DocumentNote.Note)),
                (Rule: ExceedsMaximumLength(note), Parameter: nameof(DocumentNote.Note)));
        }

        private static void ValidateDocumentNoteOnRemove(int documentId, int noteId) =>
            Validate(
                (Rule: IsInvalid(documentId), Parameter: "DocumentId"),
                (Rule: IsInvalid(noteId), Parameter: nameof(DocumentNote.Id)));

        private static void ValidateNoteIsNotNull(string note)
        {
            if (note is null)
            {
                throw new NullDocumentNoteException(message: "Document note is null.");
            }
        }

        private static dynamic IsInvalid(string text) => new
        {
            Condition = string.IsNullOrWhiteSpace(text),
            Message = "Text is required",
        };

        private static dynamic IsInvalid(int number) => new
        {
            Condition = number <= 0,
            Message = "Value must be greater than zero",
        };

        private static dynamic ExceedsMaximumLength(string text) => new
        {
            Condition = text?.Length > NoteMaximumLength,
            Message = $"Text exceeds the maximum length of {NoteMaximumLength} characters",
        };

        // Continuous validation: alle verletzten Regeln sammeln, dann den Stromkreis
        // unterbrechen -- der Aufrufer sieht jeden Fehler auf einmal.
        private static void Validate(params (dynamic Rule, string Parameter)[] validations)
        {
            var invalidDocumentNoteException =
                new InvalidDocumentNoteException(
                    message: "Document note is invalid, fix the errors and try again.");

            foreach ((dynamic rule, string parameter) in validations)
            {
                if (rule.Condition)
                {
                    invalidDocumentNoteException.UpsertDataList(
                        key: parameter,
                        value: rule.Message);
                }
            }

            invalidDocumentNoteException.ThrowIfContainsErrors();
        }
    }
}
