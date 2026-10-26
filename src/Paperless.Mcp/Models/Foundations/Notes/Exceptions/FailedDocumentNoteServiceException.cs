using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class FailedDocumentNoteServiceException : Xeption
    {
        public FailedDocumentNoteServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
