using Xeptions;

namespace Paperless.Mcp.Models.Foundations.CustomFields.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidCustomFieldException : Xeption
    {
        public InvalidCustomFieldException(string message)
            : base(message)
        { }

        public InvalidCustomFieldException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
