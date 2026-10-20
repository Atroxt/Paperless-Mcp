using Xeptions;

namespace Paperless.Mcp.Models.Foundations.GlobalSearches.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidGlobalSearchException : Xeption
    {
        public InvalidGlobalSearchException(string message)
            : base(message)
        { }

        public InvalidGlobalSearchException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
