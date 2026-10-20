using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class DocumentDependencyException : Xeption
    {
        public DocumentDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
