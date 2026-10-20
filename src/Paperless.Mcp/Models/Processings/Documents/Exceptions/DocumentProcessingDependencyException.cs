using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class DocumentProcessingDependencyException : Xeption
    {
        public DocumentProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
