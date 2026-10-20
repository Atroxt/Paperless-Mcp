using Xeptions;

namespace Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions
{
    public class DocumentTypeProcessingValidationException : Xeption
    {
        public DocumentTypeProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
