using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class ForbiddenDocumentFileException : Xeption
    {
        public ForbiddenDocumentFileException(string message)
            : base(message)
        { }
    }
}
