using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class UnauthorizedDocumentException : Xeption
    {
        public UnauthorizedDocumentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
