using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class UnauthorizedDocumentTypeException : Xeption
    {
        public UnauthorizedDocumentTypeException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
