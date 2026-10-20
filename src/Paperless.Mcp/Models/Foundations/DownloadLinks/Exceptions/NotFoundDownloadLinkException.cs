using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class NotFoundDownloadLinkException : Xeption
    {
        public NotFoundDownloadLinkException(string message)
            : base(message)
        { }
    }
}
