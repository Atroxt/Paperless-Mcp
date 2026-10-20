using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class CorrespondentProcessingDependencyValidationException : Xeption
    {
        public CorrespondentProcessingDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
