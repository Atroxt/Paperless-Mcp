using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class DocumentNoteDependencyValidationException : Xeption
    {
        public DocumentNoteDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
