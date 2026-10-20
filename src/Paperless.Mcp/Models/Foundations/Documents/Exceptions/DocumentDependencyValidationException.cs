using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class DocumentDependencyValidationException : Xeption
    {
        public DocumentDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
