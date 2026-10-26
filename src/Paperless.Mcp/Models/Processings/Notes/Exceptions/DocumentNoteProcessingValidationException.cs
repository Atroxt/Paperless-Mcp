using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    public class DocumentNoteProcessingValidationException : Xeption
    {
        public DocumentNoteProcessingValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
