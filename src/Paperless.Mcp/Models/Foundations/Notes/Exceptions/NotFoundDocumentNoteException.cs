using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class NotFoundDocumentNoteException : Xeption
    {
        public NotFoundDocumentNoteException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
