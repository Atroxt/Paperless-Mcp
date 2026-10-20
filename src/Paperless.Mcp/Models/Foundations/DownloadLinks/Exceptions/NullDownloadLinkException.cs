using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class NullDownloadLinkException : Xeption
    {
        public NullDownloadLinkException(string message)
            : base(message)
        { }
    }
}
