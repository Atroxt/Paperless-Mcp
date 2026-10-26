using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    public class DocumentNoteProcessingDependencyException : Xeption
    {
        public DocumentNoteProcessingDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
