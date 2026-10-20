using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class FailedDocumentServiceException : Xeption
    {
        public FailedDocumentServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
