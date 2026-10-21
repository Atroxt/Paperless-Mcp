using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class InvalidUploadLinkException : Xeption
    {
        public InvalidUploadLinkException(string message)
            : base(message)
        { }
    }
}
