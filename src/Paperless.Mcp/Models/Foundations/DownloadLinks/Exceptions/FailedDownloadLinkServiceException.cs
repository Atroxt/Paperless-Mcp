using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class FailedDownloadLinkServiceException : Xeption
    {
        public FailedDownloadLinkServiceException(string message, Exception innerException)
            : base(message, innerException)
        { }
    }
}
