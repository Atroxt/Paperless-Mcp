using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class DocumentTypeServiceException : Xeption
    {
        public DocumentTypeServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
