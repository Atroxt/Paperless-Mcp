using Xeptions;

namespace Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions
{
    public class DocumentTypeProcessingServiceException : Xeption
    {
        public DocumentTypeProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
