using Xeptions;

namespace Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions
{
    public class FailedDocumentTypeProcessingServiceException : Xeption
    {
        public FailedDocumentTypeProcessingServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
