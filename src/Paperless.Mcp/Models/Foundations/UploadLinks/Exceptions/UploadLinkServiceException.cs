using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class UploadLinkServiceException : Xeption
    {
        public UploadLinkServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
