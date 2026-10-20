using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Correspondents.Exceptions
{
    // Upsertable: sammelt alle verletzten Regeln, bevor der Stromkreis unterbrochen wird.
    public class InvalidCorrespondentException : Xeption
    {
        public InvalidCorrespondentException(string message)
            : base(message)
        { }

        public InvalidCorrespondentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
