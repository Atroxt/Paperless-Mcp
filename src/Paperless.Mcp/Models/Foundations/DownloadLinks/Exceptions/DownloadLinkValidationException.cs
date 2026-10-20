using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class DownloadLinkValidationException : Xeption
    {
        public DownloadLinkValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
