using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class UploadLinkValidationException : Xeption
    {
        public UploadLinkValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
