using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    public class FailedDocumentNoteProcessingServiceException : Xeption
    {
        public FailedDocumentNoteProcessingServiceException(
            string message,
            Exception innerException)
            : base(message, innerException)
        { }
    }
}
