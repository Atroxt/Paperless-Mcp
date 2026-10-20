using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class DocumentServiceException : Xeption
    {
        public DocumentServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
