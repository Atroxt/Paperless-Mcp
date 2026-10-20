using Xeptions;

namespace Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions
{
    public class DocumentTypeProcessingDependencyException : Xeption
    {
        public DocumentTypeProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
