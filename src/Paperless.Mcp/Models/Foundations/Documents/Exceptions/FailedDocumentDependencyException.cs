using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class FailedDocumentDependencyException : Xeption
    {
        public FailedDocumentDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
