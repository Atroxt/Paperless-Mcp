using Xeptions;

namespace Paperless.Mcp.Models.Processings.Correspondents.Exceptions
{
    public class CorrespondentProcessingDependencyException : Xeption
    {
        public CorrespondentProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
