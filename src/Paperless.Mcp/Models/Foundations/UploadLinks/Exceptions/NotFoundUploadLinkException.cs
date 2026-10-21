using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class NotFoundUploadLinkException : Xeption
    {
        public NotFoundUploadLinkException(string message)
            : base(message)
        { }
    }
}
