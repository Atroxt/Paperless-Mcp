using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    public class DocumentNoteProcessingDependencyValidationException : Xeption
    {
        public DocumentNoteProcessingDependencyValidationException(
            string message,
            Xeption innerException)
            : base(message, innerException)
        { }
    }
}
