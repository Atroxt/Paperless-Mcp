using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class NullDocumentProcessingException : Xeption
    {
        public NullDocumentProcessingException(string message)
            : base(message)
        { }
    }
}
