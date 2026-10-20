using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class FailedDocumentProcessingServiceException : Xeption
    {
        public FailedDocumentProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
