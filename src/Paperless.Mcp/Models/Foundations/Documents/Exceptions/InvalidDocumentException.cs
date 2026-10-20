using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidDocumentException : Xeption
    {
        public InvalidDocumentException(string message)
            : base(message)
        { }

        public InvalidDocumentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
