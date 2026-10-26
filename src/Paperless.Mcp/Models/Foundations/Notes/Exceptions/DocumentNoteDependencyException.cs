using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class DocumentNoteDependencyException : Xeption
    {
        public DocumentNoteDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
