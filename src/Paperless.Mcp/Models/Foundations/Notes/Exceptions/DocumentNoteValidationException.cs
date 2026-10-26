using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class DocumentNoteValidationException : Xeption
    {
        public DocumentNoteValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
