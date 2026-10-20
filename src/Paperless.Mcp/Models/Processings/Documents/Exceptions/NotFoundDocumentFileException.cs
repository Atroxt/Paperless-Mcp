using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class NotFoundDocumentFileException : Xeption
    {
        public NotFoundDocumentFileException(string message)
            : base(message)
        { }
    }
}
