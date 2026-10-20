using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class AlreadyExistsDocumentTypeException : Xeption
    {
        public AlreadyExistsDocumentTypeException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
