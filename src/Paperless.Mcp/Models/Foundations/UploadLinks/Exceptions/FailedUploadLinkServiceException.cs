using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class FailedUploadLinkServiceException : Xeption
    {
        public FailedUploadLinkServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
