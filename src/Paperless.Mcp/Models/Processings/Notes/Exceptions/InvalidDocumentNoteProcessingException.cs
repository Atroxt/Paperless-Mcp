using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidDocumentNoteProcessingException : Xeption
    {
        public InvalidDocumentNoteProcessingException(string message)
            : base(message)
        { }

        public InvalidDocumentNoteProcessingException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
