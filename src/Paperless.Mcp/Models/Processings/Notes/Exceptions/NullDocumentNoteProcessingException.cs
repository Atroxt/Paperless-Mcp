using Xeptions;

namespace Paperless.Mcp.Models.Processings.Notes.Exceptions
{
    public class NullDocumentNoteProcessingException : Xeption
    {
        public NullDocumentNoteProcessingException(string message)
            : base(message)
        { }
    }
}
