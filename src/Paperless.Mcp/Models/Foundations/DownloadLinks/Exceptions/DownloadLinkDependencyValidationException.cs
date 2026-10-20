using Xeptions;

namespace Paperless.Mcp.Models.Foundations.DownloadLinks.Exceptions
{
    public class DownloadLinkDependencyValidationException : Xeption
    {
        public DownloadLinkDependencyValidationException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
