using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Notes.Exceptions
{
    public class NullDocumentNoteException : Xeption
    {
        public NullDocumentNoteException(string message)
            : base(message)
        { }
    }
}
