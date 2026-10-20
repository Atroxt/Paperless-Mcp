using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Documents.Exceptions
{
    public class AlreadyExistsDocumentException : Xeption
    {
        public AlreadyExistsDocumentException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
