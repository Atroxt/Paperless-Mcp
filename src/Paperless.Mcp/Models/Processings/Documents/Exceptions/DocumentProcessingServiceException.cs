using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class DocumentProcessingServiceException : Xeption
    {
        public DocumentProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
