using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class NotFoundDocumentException : Xeption
    {
        public NotFoundDocumentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
