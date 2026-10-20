using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class CorrespondentProcessingValidationException : Xeption
    {
        public CorrespondentProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
