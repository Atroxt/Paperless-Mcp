using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidDocumentTypeException : Xeption
    {
        public InvalidDocumentTypeException(string message)
            : base(message)
        { }

        public InvalidDocumentTypeException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
