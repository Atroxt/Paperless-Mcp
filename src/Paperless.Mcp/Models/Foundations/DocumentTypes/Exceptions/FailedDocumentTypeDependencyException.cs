using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class FailedDocumentTypeDependencyException : Xeption
    {
        public FailedDocumentTypeDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
