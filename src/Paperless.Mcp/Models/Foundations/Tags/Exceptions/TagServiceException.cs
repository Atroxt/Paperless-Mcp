using Xeptions;

namespace Paperless.Mcp.Models.Foundations.Tags.Exceptions
{
    public class TagServiceException : Xeption
    {
        public TagServiceException(string message, Xeption innerException)
            : base(message, innerException)
        { }
    }
}
