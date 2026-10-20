using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DocumentTypes.Exceptions
{
    public class DocumentTypeDependencyException : Xeption
    {
        public DocumentTypeDependencyException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
