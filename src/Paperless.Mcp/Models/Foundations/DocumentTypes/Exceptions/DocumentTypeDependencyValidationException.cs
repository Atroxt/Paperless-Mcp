using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class DocumentTypeDependencyValidationException : Xeption
    {
        public DocumentTypeDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
