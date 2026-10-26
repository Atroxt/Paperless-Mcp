using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class UnauthorizedDocumentNoteException : Xeption
    {
        public UnauthorizedDocumentNoteException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
