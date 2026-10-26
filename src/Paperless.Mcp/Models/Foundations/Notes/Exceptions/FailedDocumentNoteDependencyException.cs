using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class FailedDocumentNoteDependencyException : Xeption
    {
        public FailedDocumentNoteDependencyException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
