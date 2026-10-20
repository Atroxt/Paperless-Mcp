using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class DownloadLinkServiceException : Xeption
    {
        public DownloadLinkServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
