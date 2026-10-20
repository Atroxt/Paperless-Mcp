using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class DocumentProcessingDependencyValidationException : Xeption
    {
        public DocumentProcessingDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
