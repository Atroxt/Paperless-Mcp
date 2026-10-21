using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class UploadLinkDependencyValidationException : Xeption
    {
        public UploadLinkDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
