using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class NullDocumentException : Xeption
    {
        public NullDocumentException(string message)
            : base(message)
        { }
    }
}
