using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidDocumentNoteException : Xeption
    {
        public InvalidDocumentNoteException(string message)
            : base(message)
        { }

        public InvalidDocumentNoteException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
