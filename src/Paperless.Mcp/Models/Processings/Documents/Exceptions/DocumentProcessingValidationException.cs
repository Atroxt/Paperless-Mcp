using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class DocumentProcessingValidationException : Xeption
    {
        public DocumentProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
