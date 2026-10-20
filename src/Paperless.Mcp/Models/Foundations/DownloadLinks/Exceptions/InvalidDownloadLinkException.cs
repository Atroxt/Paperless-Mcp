using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class InvalidDownloadLinkException : Xeption
    {
        public InvalidDownloadLinkException(string message)
            : base(message)
        { }
    }
}
