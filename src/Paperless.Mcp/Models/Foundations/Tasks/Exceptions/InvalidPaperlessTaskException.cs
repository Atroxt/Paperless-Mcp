using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tasks.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidPaperlessTaskException : Xeption
    {
        public InvalidPaperlessTaskException(string message)
            : base(message)
        { }

        public InvalidPaperlessTaskException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
