using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class FailedDocumentTypeServiceException : Xeption
    {
        public FailedDocumentTypeServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
