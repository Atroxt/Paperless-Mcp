using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class DocumentValidationException : Xeption
    {
        public DocumentValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
