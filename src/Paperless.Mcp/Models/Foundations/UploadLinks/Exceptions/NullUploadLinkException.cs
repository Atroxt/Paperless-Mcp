using Xeptions;

namespace Paperless.Mcp.Models.Foundations.UploadLinks.Exceptions
{
    public class NullUploadLinkException : Xeption
    {
        public NullUploadLinkException(string message)
            : base(message)
        { }
    }
}
