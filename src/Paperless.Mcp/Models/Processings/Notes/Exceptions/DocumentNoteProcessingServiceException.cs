using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    public class DocumentNoteProcessingServiceException : Xeption
    {
        public DocumentNoteProcessingServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
