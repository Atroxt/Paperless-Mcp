using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidTagException : Xeption
    {
        public InvalidTagException(string message)
            : base(message)
        { }

        public InvalidTagException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
