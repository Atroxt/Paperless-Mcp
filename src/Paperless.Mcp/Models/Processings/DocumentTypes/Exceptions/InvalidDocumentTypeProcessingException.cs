using Xeptions;

namespace Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions
{
    public class InvalidDocumentTypeProcessingException : Xeption
    {
        public InvalidDocumentTypeProcessingException(string message)
            : base(message)
        { }
    }
}
