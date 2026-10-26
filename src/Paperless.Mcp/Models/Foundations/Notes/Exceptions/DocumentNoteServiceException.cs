using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class DocumentNoteServiceException : Xeption
    {
        public DocumentNoteServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
