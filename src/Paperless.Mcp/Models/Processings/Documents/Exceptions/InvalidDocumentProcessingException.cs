using Xeptions;

namespace Paperless.Mcp.Models.Processings.Documents.Exceptions
{
    public class InvalidDocumentProcessingException : Xeption
    {
        public InvalidDocumentProcessingException(string message)
            : base(message)
        { }
    }
}
