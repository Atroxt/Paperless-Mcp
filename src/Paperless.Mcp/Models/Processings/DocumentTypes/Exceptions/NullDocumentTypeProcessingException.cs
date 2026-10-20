using Xeptions;

namespace Paperless.Mcp.Models.Processings.DocumentTypes.Exceptions
{
    public class NullDocumentTypeProcessingException : Xeption
    {
        public NullDocumentTypeProcessingException(string message)
            : base(message)
        { }
    }
}
