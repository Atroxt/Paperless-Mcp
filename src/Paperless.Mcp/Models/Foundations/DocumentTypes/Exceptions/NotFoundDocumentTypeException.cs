using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class NotFoundDocumentTypeException : Xeption
    {
        public NotFoundDocumentTypeException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
