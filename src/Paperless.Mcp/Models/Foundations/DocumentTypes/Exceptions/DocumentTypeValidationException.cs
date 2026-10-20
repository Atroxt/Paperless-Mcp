using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class DocumentTypeValidationException : Xeption
    {
        public DocumentTypeValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
